using Amazon.DynamoDBv2.DataModel;
using Goldlight.Database.Models.v1;

namespace Goldlight.Database.DatabaseOperations;

public class ProjectDataAccess
{
  private readonly IDynamoDBContext dynamoDbContext;
  private readonly PostgresConnection postgresConnection;

  public ProjectDataAccess(IDynamoDBContext dbContext, PostgresConnection postgresConnection)
  {
    dynamoDbContext = dbContext;
    this.postgresConnection = postgresConnection;
  }

  public virtual async Task SaveProjectAsync(ProjectTable project)
  {
    await dynamoDbContext.SaveAsync(project);
  }

  public virtual async Task<IEnumerable<ProjectTable>> GetProjectsAsync(string organization)
  {
    DynamoDBOperationConfig queryOperationConfig = new()
    {
      IndexName = "project-organization_id-index"
    };

    List<ProjectTable> projects = new();
    AsyncSearch<ProjectTable>? search = dynamoDbContext.QueryAsync<ProjectTable>(organization, queryOperationConfig);
    while (!search.IsDone)
    {
      projects.AddRange(await search.GetNextSetAsync());
    }

    return projects;
  }

  public virtual async Task DeleteProjectAsync(string id)
  {
    await dynamoDbContext.DeleteAsync<ProjectTable>(id);
  }
}