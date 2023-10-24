using Amazon.DynamoDBv2.DataModel;
using Goldlight.Database.Models.v1.RequestResponse;

namespace Goldlight.Database.Models.v1;

public class Details
{
  [DynamoDBProperty] public string Name { get; set; } = null!;
  [DynamoDBProperty] public string Description { get; set; } = null!;
  [DynamoDBProperty] public string FriendlyName { get; set; } = null!;
  [DynamoDBProperty("rrpairs")]
  public RequestResponsePairTableFragment[]? RequestResponsePairs { get; set; }
}