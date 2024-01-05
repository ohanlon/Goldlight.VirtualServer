using System.Data;
using Dapper;
using Goldlight.ExceptionManagement;
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

  public virtual async Task AddUserToOrganization(Guid organization, string email, string role)
  {
    DynamicParameters dynamicParameters = new();
    dynamicParameters.Add("p_orgid", value: organization, dbType: DbType.Guid);
    dynamicParameters.Add("p_email", value: email, DbType.AnsiString);
    dynamicParameters.Add("p_role", value: role.ToUpper(), DbType.AnsiString);
    dynamicParameters.Add("affected_rows", value: 0, dbType: DbType.Int64,
      direction: ParameterDirection.Output);
    _ = await ExecuteStoredProcedureAsync("sv.\"glsp_SaveUserRoles\"", dynamicParameters, () =>
    {
      if (dynamicParameters.Get<int>("affected_rows") == 0)
      {
        throw new NotFoundException();
      }
    });
  }

  public virtual async Task<IEnumerable<Organization>> GetOrganizationsAsync(string email)
  {
    using var connection = Connection;
    return await connection.QueryAsync<Organization>(
      $"SELECT DISTINCT id, friendlyname, name, apikey, version FROM sv.\"organization_users\" WHERE userid=@email",
      new { email });
  }

  public virtual async Task<IEnumerable<OrganizationMember>> GetMembers(Guid organizationId)
  {
    SetTypeMap(typeof(OrganizationMember));
    using var connection = Connection;
    return await connection.QueryAsync<OrganizationMember>(
      "SELECT DISTINCT userid, rolename FROM sv.\"organization_users\" WHERE id=@organizationId",
      new { organizationId });
  }

  public virtual async Task ValidateCurrentUserIsPresentInOrganization(Guid organizationId, string email)
  {
    using var connection = Connection;
    var organizations = await connection.QueryAsync<Organization>(
      $"SELECT id, friendlyname, name, apikey, version FROM sv.\"organization_users\" WHERE id=@organizationId AND userid=@email",
      new { organizationId, email });
    if (!organizations.Any())
    {
      throw new UserNotMemberOfOrganizationException();
    }
  }

  public virtual async Task ValidateCurrentUserIsPresentInOrganization(string friendlyName, string email)
  {
    using var connection = Connection;
    var organizations = await connection.QueryAsync<Organization>(
      $"SELECT id, friendlyname, name, apikey, version FROM sv.\"organization_users\" WHERE friendlyname=@friendlyName AND userid=@email",
      new { friendlyName, email });
    if (!organizations.Any())
    {
      throw new UserNotMemberOfOrganizationException();
    }
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