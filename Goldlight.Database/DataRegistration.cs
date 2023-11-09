using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Goldlight.Database.DatabaseOperations;
using LocalStack.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Goldlight.Database;

public static class DataRegistration
{
  public static IServiceCollection AddData(this IServiceCollection services)
  {
    return services.AddTransient<OrganizationDataAccess>().AddAwsService<IAmazonDynamoDB>()
      .AddTransient<IDynamoDBContext, DynamoDBContext>()
      .AddTransient<ProjectDataAccess>()
      .AddTransient<UserDataAccess>()
      .AddTransient<DatabaseMigrationDataAccess>()
      .AddSingleton<PostgresConnection>();
  }
}