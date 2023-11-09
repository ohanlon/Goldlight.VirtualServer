using System.Data;
using Dapper;

namespace Goldlight.Database.DatabaseOperations;

public class UserDataAccess
{
  private readonly PostgresConnection postgresConnection;

  public UserDataAccess(PostgresConnection postgresConnection)
  {
    this.postgresConnection = postgresConnection;
  }

  public async Task AddUser(string emailAddress)
  {
    using var connection = postgresConnection.Connection;
    DynamicParameters dynamicParameters = new();
    dynamicParameters.Add("p_email", emailAddress);
    _ = await connection.ExecuteAsync("sv.\"\"", dynamicParameters, commandType: CommandType.StoredProcedure);
  }
}