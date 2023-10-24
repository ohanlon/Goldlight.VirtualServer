using Amazon.DynamoDBv2.DataModel;

namespace Goldlight.Database.Models.v1.RequestResponse;

public class RequestResponsePairTableFragment
{
  [DynamoDBProperty] public string Name { get; set; } = null!;
  [DynamoDBProperty] public string Description { get; set; } = null!;
  [DynamoDBProperty] public RequestTableFragment Request { get; set; } = null!;
  [DynamoDBProperty] public ResponseTableFragment Response { get; set; } = null!;
}