using System.Data;
using System.Runtime.Serialization;
using Dapper;

namespace Goldlight.Database.DatabaseOperations;

public abstract class BaseDataAccess
{
  private readonly PostgresConnection connection;

  protected BaseDataAccess(PostgresConnection postgresConnection)
  {
    connection = postgresConnection;
  }

  protected IDbConnection Connection => connection.Connection;

  protected async Task<int> ExecuteCommandAsync(string sql, object parameters, Action? preCommit = null) =>
    await ExecuteAsync(sql, parameters, preCommit, CommandType.Text).ConfigureAwait(false);

  protected async Task<int> ExecuteStoredProcedureAsync(string sql, object parameters, Action? preCommit = null) =>
    await ExecuteAsync(sql, parameters, preCommit);

  protected void SetTypeMap(Type type)
  {
    SqlMapper.SetTypeMap(type, new CustomPropertyTypeMap(
      type, (type, columnName) => type.GetProperties().FirstOrDefault(prop =>
        prop.GetCustomAttributes(false).OfType<DataMemberAttribute>().Any(attr => attr.Name == columnName))!));
  }

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
    catch (Exception)
    {
      transaction.Rollback();
      throw;
    }
  }
}