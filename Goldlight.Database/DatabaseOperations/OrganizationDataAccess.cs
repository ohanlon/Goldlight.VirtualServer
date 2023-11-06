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

  public virtual async Task<Organization> SaveAsync(Organization organization)
  {
    using var connection = postgresConnection.Connection;
    DynamicParameters dynamicParameters = new DynamicParameters();
    dynamicParameters.Add("p_id", value: organization.Id, dbType: DbType.Guid);
    dynamicParameters.Add("p_friendlyname", value: organization.FriendlyName, dbType: DbType.String);
    dynamicParameters.Add("p_name", value: organization.Name, dbType: DbType.String);
    dynamicParameters.Add("p_apikey", value: organization.ApiKey, dbType: DbType.String);
    dynamicParameters.Add("p_version", value: organization.Version, dbType: DbType.String,
      direction: ParameterDirection.InputOutput);
    dynamicParameters.Add("affected_rows", dbType: DbType.String, direction: ParameterDirection.Output);
    var results = await connection.ExecuteAsync("glsp_InsertOrganization",
      new
      {
        p_id = organization.Id, p_friendlyname = organization.FriendlyName, p_name = organization.Name,
        p_apikey = organization.ApiKey, p_version = organization.Version
      }, commandType: CommandType.StoredProcedure);
    int output = dynamicParameters.Get<int>("affected_Rows");
    if (output is 0) throw new SaveConflictException();
    return organization;
  }

  public virtual async Task<IEnumerable<Organization>> GetOrganizationsAsync()
  {
    using IDbConnection connection = postgresConnection.Connection;
    return await connection.QueryAsync<Organization>(
      "SELECT id, friendlyname, name, apikey, version FROM sv.\"Organization\"");
  }

  public virtual async Task<Organization?> GetOrganizationAsync(Guid id)
  {
    using IDbConnection connection = postgresConnection.Connection;
    IEnumerable<Organization> organizations = await connection.QueryAsync<Organization>(
      "SELECT id, friendlyname, name, apikey, version FROM sv.\"Organization\" WHERE id=id",
      new { id = id.ToString() });
    return organizations.FirstOrDefault();
  }

  public virtual async Task<Organization?> GetOrganizationByNameAsync(string name)
  {
    using IDbConnection connection = postgresConnection.Connection;
    IEnumerable<Organization> organizations = await connection.QueryAsync<Organization>(
      "SELECT id, friendlyname, name, apikey, version FROM sv.\"Organization\" WHERE name=name", new { name });
    return organizations.FirstOrDefault();
  }


  public virtual async Task DeleteOrganizationAsync(string id)
  {
    await dynamoDbContext.DeleteAsync<OrganizationTable>(id);
  }
}