using Amazon.DynamoDBv2.DataModel;
using Goldlight.Database.Models.v1.RequestResponse;

namespace Goldlight.Database.Models.v1;

[DynamoDBTable("projects")]
public class ProjectTable
{
  [DynamoDBHashKey("id")]
  public string Id { get; set; } = "";
  [DynamoDBProperty("organization_id")]
  public string OrganizationId { get; set; } = "";
  [DynamoDBProperty]
  public int ModelVersion { get; set; } = 1;
  [DynamoDBVersion]
  public long? Version { get; set; }
  [DynamoDBProperty("details")] 
  public Details? Details { get; set; }
}