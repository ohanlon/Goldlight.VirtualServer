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
    _ = await ExecuteStoredProcedureAsync("sv.\"\"", dynamicParameters);
  }
}