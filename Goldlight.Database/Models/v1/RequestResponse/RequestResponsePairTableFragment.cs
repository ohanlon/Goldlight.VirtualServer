using Amazon.DynamoDBv2.DataModel;

namespace Goldlight.Database.Models.v1.RequestResponse;

public class RequestResponsePairTableFragment
{
  [DynamoDBProperty] public string? Name { get; set; }
  [DynamoDBProperty] public string? Description { get; set; }
  [DynamoDBProperty] public RequestTableFragment Request = new();
  [DynamoDBProperty] public ResponseTableFragment Response = new();
}