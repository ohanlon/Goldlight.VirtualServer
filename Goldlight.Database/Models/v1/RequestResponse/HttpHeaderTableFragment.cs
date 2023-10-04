using Amazon.DynamoDBv2.DataModel;

namespace Goldlight.Database.Models.v1.RequestResponse;
public class HttpHeaderTableFragment
{
  [DynamoDBProperty] public string? Name { get; set; }
  [DynamoDBProperty] public string? Value { get; set; }
}