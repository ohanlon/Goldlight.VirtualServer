using System.Data;
using Dapper;

namespace Goldlight.Database.DatabaseOperations;

public class DatabaseMigrationDataAccess
{
  private readonly PostgresConnection postgresConnection;

  public DatabaseMigrationDataAccess(PostgresConnection postgresConnection)
  {
    this.postgresConnection = postgresConnection;
  }

  public async Task MigrateDatabaseAsync()
  {
    using var connection = postgresConnection.Connection;
    await connection.ExecuteAsync("sv.\"glsp_InsertRoles\"", commandType: CommandType.StoredProcedure);
  }
}