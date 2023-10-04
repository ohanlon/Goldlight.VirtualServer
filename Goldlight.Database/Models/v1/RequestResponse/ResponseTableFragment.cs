using Amazon.DynamoDBv2.DataModel;

namespace Goldlight.Database.Models.v1.RequestResponse;

public class ResponseTableFragment
{
  [DynamoDBProperty] public HttpResponseSummaryTableFragment Summary { get; set; } = null!;
  [DynamoDBProperty]
  public HttpHeaderTableFragment[]? Headers { get; set; }

  [DynamoDBProperty]
  public string? Content { get; set; }
}