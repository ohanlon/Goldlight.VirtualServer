using Amazon.DynamoDBv2.DataModel;

namespace Goldlight.Database.Models.v1.RequestResponse;

public class HttpResponseSummaryTableFragment
{
  [DynamoDBProperty] public string? Version { get; set; }
  [DynamoDBProperty] public string? Status { get; set; }
}