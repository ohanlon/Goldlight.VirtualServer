using Goldlight.Database.Models.v1;
using Amazon.DynamoDBv2.DataModel;

namespace Goldlight.Database.DatabaseOperations;

public class OrganizationDataAccess
{
  private readonly IDynamoDBContext _dynamoDbContext;
  public OrganizationDataAccess(IDynamoDBContext dbContext)
  {
    _dynamoDbContext = dbContext;
  }

  public virtual async Task SaveOrganizationAsync(OrganizationTable organization)
  {
    await _dynamoDbContext.SaveAsync(organization);
  }

  public virtual async Task<IEnumerable<OrganizationTable>> GetOrganizationsAsync()
  {
    return await _dynamoDbContext.ScanAsync<OrganizationTable>(new List<ScanCondition>()).GetRemainingAsync();
  }
}