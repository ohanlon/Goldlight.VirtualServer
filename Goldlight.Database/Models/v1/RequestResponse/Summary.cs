using Amazon.DynamoDBv2.DataModel;

namespace Goldlight.Database.Models.v1.RequestResponse;

public class Summary
{
  [DynamoDBProperty] public string Method { get; set; } = null!;
  [DynamoDBProperty] public string Path { get; set; } = null!;
  [DynamoDBProperty] public string? Protocol { get; set; }
}