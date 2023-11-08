using System.Data;
using Goldlight.Database.Models.v1;
using Amazon.DynamoDBv2.DataModel;
using Dapper;
using Goldlight.Database.Exceptions;
using Goldlight.Models;

namespace Goldlight.Database.DatabaseOperations;

public class OrganizationDataAccess
{
  private readonly IDynamoDBContext dynamoDbContext;
  private readonly PostgresConnection postgresConnection;

  public OrganizationDataAccess(IDynamoDBContext dbContext, PostgresConnection postgresConnection)
  {
    dynamoDbContext = dbContext;
    this.postgresConnection = postgresConnection;
  }

  public virtual async Task<Organization> SaveAsync(Organization organization, string emailAddress)
  {
    using var connection = postgresConnection.Connection;
    int affectedRows = 0;
    DynamicParameters dynamicParameters = new();
    dynamicParameters.Add("p_id", value: organization.Id, dbType: DbType.Guid);
    dynamicParameters.Add("p_friendlyname", value: organization.FriendlyName.ToLowerInvariant(), dbType: DbType.String);
    dynamicParameters.Add("p_name", value: organization.Name, dbType: DbType.String);
    dynamicParameters.Add("p_apikey", value: organization.ApiKey, dbType: DbType.String);
    dynamicParameters.Add("p_email", value: emailAddress, dbType: DbType.String);
    dynamicParameters.Add("p_version", value: organization.Version, dbType: DbType.Int64,
      direction: ParameterDirection.InputOutput);
    dynamicParameters.Add("affected_rows", value: affectedRows, dbType: DbType.Int64,
      direction: ParameterDirection.Output);
    _ = await connection.ExecuteAsync("sv.\"glsp_InsertOrganization\"",
      dynamicParameters, commandType: CommandType.StoredProcedure);
    if (dynamicParameters.Get<int>("affected_rows") == 0)
    {
      throw new SaveConflictException();
    }

    organization.Version = dynamicParameters.Get<long>("p_version");
    return organization;
  }

  public virtual async Task<IEnumerable<Organization>> GetOrganizationsAsync(string email)
  {
    using IDbConnection connection = postgresConnection.Connection;
    return await connection.QueryAsync<Organization>(
      $"SELECT id, friendlyname, name, apikey, version FROM sv.\"organization_users\" WHERE userid=@email",
      new { email });
  }

  public virtual async Task<Organization?> GetOrganizationAsync(Guid id)
  {
    using IDbConnection connection = postgresConnection.Connection;
    IEnumerable<Organization> organizations = await connection.QueryAsync<Organization>(
      "SELECT id, friendlyname, name, apikey, version FROM sv.\"Organization\" WHERE id=@id",
      new { id = id.ToString() });
    return organizations.FirstOrDefault();
  }

  public virtual async Task<Organization?> GetOrganizationByNameAsync(string name)
  {
    using IDbConnection connection = postgresConnection.Connection;
    IEnumerable<Organization> organizations = await connection.QueryAsync<Organization>(
      "SELECT id, friendlyname, name, apikey, version FROM sv.\"Organization\" WHERE friendlyname=@name", new { name });
    return organizations.FirstOrDefault();
  }


  public virtual async Task DeleteOrganizationAsync(string id)
  {
    await dynamoDbContext.DeleteAsync<OrganizationTable>(id);
  }
}