using Goldlight.Database.Models.v1;
using Amazon.DynamoDBv2.DataModel;

namespace Goldlight.Database.DatabaseOperations;

public class OrganizationDataAccess
{
  private readonly IDynamoDBContext dynamoDbContext;
  public OrganizationDataAccess(IDynamoDBContext dbContext)
  {
    dynamoDbContext = dbContext;
  }

  public virtual async Task SaveOrganizationAsync(OrganizationTable organization)
  {
    await dynamoDbContext.SaveAsync(organization);
  }

  public virtual async Task<IEnumerable<OrganizationTable>> GetOrganizationsAsync()
  {
    List<OrganizationTable> organizations = new(100);
    AsyncSearch<OrganizationTable>? search = dynamoDbContext.ScanAsync<OrganizationTable>(new List<ScanCondition>());

    while (!search.IsDone)
    {
      organizations.AddRange(await search.GetNextSetAsync());
    }

    return organizations;
  }

  public virtual async Task<OrganizationTable?> GetOrganizationAsync(string id)
  {
    return await dynamoDbContext.LoadAsync<OrganizationTable>(id);
  }

  public virtual async Task DeleteOrganizationAsync(string id)
  {
    await dynamoDbContext.DeleteAsync<OrganizationTable>(id);
  }
}