using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Goldlight.Database.DatabaseOperations;

public class PostgresConnection
{
  private readonly string connectionString;

  public PostgresConnection(IConfiguration configuration)
  {
    string host = configuration.GetSection("Postgres:host").Value!;
    string port = configuration.GetSection("Postgres:port").Value!;
    string user = configuration.GetSection("Postgres:user").Value!;
    string password = configuration.GetSection("Postgres:password").Value!;
    string database = configuration.GetSection("Postgres:database").Value!;
    connectionString =
      $"User ID={user};Password={password};Host={host};Port={port};Database={database}";
  }

  public virtual IDbConnection Connection => new NpgsqlConnection(connectionString);
}