using System.Data;
using Dapper;
using Goldlight.Database.Exceptions;
using Goldlight.Models;

namespace Goldlight.Database.DatabaseOperations;

public class OrganizationDataAccess : BaseDataAccess
{
  public OrganizationDataAccess(PostgresConnection postgresConnection) : base(postgresConnection)
  {
  }

  public virtual async Task<Organization> SaveAsync(Organization organization, string emailAddress)
  {
    var dynamicParameters = BuildSaveOrganizationDynamicParameters(organization, emailAddress);
    _ = await ExecuteStoredProcedureAsync("sv.\"glsp_SaveOrganization\"",
      dynamicParameters, () =>
      {
        if (dynamicParameters.Get<int>("affected_rows") == 0)
        {
          throw new SaveConflictException();
        }
      });

    organization.Version = dynamicParameters.Get<long>("p_version");
    return organization;
  }

  public virtual async Task<IEnumerable<Organization>> GetOrganizationsAsync(string email)
  {
    using IDbConnection connection = Connection;
    return await connection.QueryAsync<Organization>(
      $"SELECT id, friendlyname, name, apikey, version FROM sv.\"organization_users\" WHERE userid=@email",
      new { email });
  }

  public virtual async Task<bool> IsUserPresentInOrganization(Guid organizationId, string email)
  {
    using IDbConnection connection = Connection;
    var organizations = await connection.QueryAsync<Organization>(
      $"SELECT id, friendlyname, name, apikey, version FROM sv.\"organization_users\" WHERE id=@id AND userid=@email",
      new { organizationId, email });
    return !organizations.Any();
  }

  public virtual async Task<bool> IsUserPresentInOrganization(string friendlyName, string email)
  {
    using IDbConnection connection = Connection;
    var organizations = await connection.QueryAsync<Organization>(
      $"SELECT id, friendlyname, name, apikey, version FROM sv.\"organization_users\" WHERE friendlyname=@friendlyName AND userid=@email",
      new { friendlyName, email });
    return !organizations.Any();
  }

  public virtual async Task<Organization?> GetOrganizationAsync(Guid id)
  {
    using IDbConnection connection = Connection;
    IEnumerable<Organization> organizations = await connection.QueryAsync<Organization>(
      "SELECT id, friendlyname, name, apikey, version FROM sv.\"Organization\" WHERE id=@id",
      new { id = id.ToString() });
    return organizations.FirstOrDefault();
  }

  public virtual async Task<Organization?> GetOrganizationByNameAsync(string name)
  {
    using IDbConnection connection = Connection;
    IEnumerable<Organization> organizations = await connection.QueryAsync<Organization>(
      "SELECT id, friendlyname, name, apikey, version FROM sv.\"Organization\" WHERE friendlyname=@name", new { name });
    return organizations.FirstOrDefault();
  }

  private static DynamicParameters BuildSaveOrganizationDynamicParameters(Organization organization,
    string emailAddress)
  {
    DynamicParameters dynamicParameters = new();
    dynamicParameters.Add("p_id", value: organization.Id, dbType: DbType.Guid);
    dynamicParameters.Add("p_friendlyname", value: organization.FriendlyName.ToLowerInvariant(), dbType: DbType.String);
    dynamicParameters.Add("p_name", value: organization.Name, dbType: DbType.String);
    dynamicParameters.Add("p_apikey", value: organization.ApiKey, dbType: DbType.String);
    dynamicParameters.Add("p_email", value: emailAddress, dbType: DbType.String);
    dynamicParameters.Add("p_version", value: organization.Version, dbType: DbType.Int64,
      direction: ParameterDirection.InputOutput);
    dynamicParameters.Add("affected_rows", value: 0, dbType: DbType.Int64,
      direction: ParameterDirection.Output);
    return dynamicParameters;
  }
}