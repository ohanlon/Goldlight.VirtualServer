using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Goldlight.Database.Models.v1;

namespace Goldlight.Database.DatabaseOperations;

public class ProjectDataAccess
{
  private readonly IDynamoDBContext dynamoDbContext;
  public ProjectDataAccess(IDynamoDBContext dbContext)
  {
    dynamoDbContext = dbContext;
  }

  public virtual async Task SaveProjectAsync(ProjectTable project)
  {
    await dynamoDbContext.SaveAsync(project);
  }

  public virtual async Task<IEnumerable<ProjectTable>> GetProjectsAsync(string organization)
  {
    DynamoDBOperationConfig queryOperationConfig = new DynamoDBOperationConfig
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
}