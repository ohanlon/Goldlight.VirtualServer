using Goldlight.Database.DatabaseOperations;
using Microsoft.Extensions.DependencyInjection;

namespace Goldlight.Database;

public static class DataRegistration
{
  public static IServiceCollection AddData(this IServiceCollection services)
  {
    return services.AddTransient<OrganizationDataAccess>()
      .AddTransient<ProjectDataAccess>()
      .AddTransient<UserDataAccess>()
      .AddTransient<DatabaseMigrationDataAccess>()
      .AddSingleton<PostgresConnection>();
  }
}