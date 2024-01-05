using Dapper;

namespace Goldlight.Database.DatabaseOperations;

public class UserDataAccess : BaseDataAccess
{
  public UserDataAccess(PostgresConnection postgresConnection) : base(postgresConnection)
  {
  }

  public async Task AddUser(string emailAddress)
  {
    DynamicParameters dynamicParameters = new();
    dynamicParameters.Add("p_email", emailAddress);
    _ = await ExecuteStoredProcedureAsync("sv.\"glsp_AddUser\"", dynamicParameters);
  }

  public async Task<bool> UserInOrganization(string emailAddress, Guid organization)
  {
    var count = await Connection.QueryFirstAsync<int>(
      "SELECT COUNT(1) FROM sv.\"organization_users\" WHERE userid=@emailAddress AND id=@organization",
      new { emailAddress, organization });
    return count > 0;
  }

  public async Task<string?> UserRoleForOrganization(string emailAddress, Guid organization)
  {
    var rolename = await Connection.QueryFirstAsync<string>(
      "SELECT rolename FROM sv.\"organization_users\" WHERE userid=@emailAddress AND id=@organization",
      new { emailAddress, organization });
    return rolename;
  }

  public async Task DeleteUserFromOrganization(string emailAddress, Guid organization)
  {
    await Connection.ExecuteAsync(
      "DELETE FROM sv.\"OrganizationUser\" WHERE organization_id=@organization AND user_id IN (SELECT id FROM sv.\"User\" WHERE userid=@emailAddress)",
      new { emailAddress, organization });
  }
}