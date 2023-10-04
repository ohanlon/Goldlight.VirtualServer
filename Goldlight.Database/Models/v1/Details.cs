using Amazon.DynamoDBv2.DataModel;
using Goldlight.Database.Models.v1.RequestResponse;

namespace Goldlight.Database.Models.v1;

public class Details
{
  [DynamoDBProperty]
  public string Name { get; set; }
  [DynamoDBProperty]
  public string Description { get; set; }
  [DynamoDBProperty]
  public string FriendlyName { get; set; }
  [DynamoDBProperty("rrpairs")]
  public RequestResponsePairTableFragment[]? RequestResponsePairs { get; set; }
}