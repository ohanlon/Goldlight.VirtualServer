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

  public async Task<string?> UserRoleForProject(string emailAddress, Guid project)
  {
    var rolename = await Connection.QueryFirstAsync<string>(
      "SELECT rolename FROM sv.\"organization_users\" WHERE userid=@emailAddress AND project=@project",
      new { emailAddress, project });
    return rolename;
  }
}