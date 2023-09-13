using Amazon.DynamoDBv2.DataModel;

namespace Goldlight.Database.Models.v1;

public class Details
{
  [DynamoDBProperty]
  public string Name { get; set; }
  [DynamoDBProperty]
  public string Description { get; set; }
  [DynamoDBProperty]
  public string FriendlyName { get; set; }
}