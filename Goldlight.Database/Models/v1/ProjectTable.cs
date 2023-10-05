using Amazon.DynamoDBv2.DataModel;

namespace Goldlight.Database.Models.v1;

[DynamoDBTable("projects")]
public class ProjectTable
{
  [DynamoDBHashKey("id")] public string Id { get; set; } = null!;
  [DynamoDBProperty("organization_id")] public string OrganizationId { get; set; } = null!;
  [DynamoDBProperty] public int ModelVersion { get; set; } = 1;
  [DynamoDBVersion] public long? Version { get; set; }
  [DynamoDBProperty("details")] public Details Details { get; set; } = null!;
}