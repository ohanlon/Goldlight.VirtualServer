using System.Data;
using Dapper;

namespace Goldlight.Database.DatabaseOperations;

public abstract class BaseDataAccess
{
  private readonly PostgresConnection postgresConnection;

  protected BaseDataAccess(PostgresConnection postgresConnection)
  {
    this.postgresConnection = postgresConnection;
  }

  protected IDbConnection Connection => postgresConnection.Connection;

  protected async Task<int> ExecuteCommandAsync(string sql, object parameters, Action? preCommit = null) =>
    await ExecuteAsync(sql, parameters, preCommit, CommandType.Text).ConfigureAwait(false);

  protected async Task<int> ExecuteStoredProcedureAsync(string sql, object parameters, Action? preCommit = null) =>
    await ExecuteAsync(sql, parameters, preCommit);

  private async Task<int> ExecuteAsync(string sql, object parameters, Action? preCommit = null,
    CommandType commandType = CommandType.StoredProcedure)
  {
    using var connection = Connection;
    connection.Open();
    using IDbTransaction transaction = connection.BeginTransaction();

    try
    {
      int affected =
        await connection.ExecuteAsync(sql, parameters, transaction, commandType: commandType);
      preCommit?.Invoke();
      transaction.Commit();
      return affected;
    }
    catch (Exception ex)
    {
      transaction.Rollback();
      throw;
    }
  }
}