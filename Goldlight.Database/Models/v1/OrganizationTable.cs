using Amazon.DynamoDBv2.DataModel;

namespace Goldlight.Database.Models.v1;

[DynamoDBTable("organizations")]
public class OrganizationTable
{
  [DynamoDBHashKey("id")] 
  public string? Id { get; set; }
  [DynamoDBProperty] 
  public string? Name { get; set; }
  [DynamoDBProperty] 
  public int ModelVersion { get; set; } = 1;
  [DynamoDBVersion] 
  public long? Version { get; set; }
}