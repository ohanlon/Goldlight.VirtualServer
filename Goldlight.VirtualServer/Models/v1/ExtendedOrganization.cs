using System.Runtime.Serialization;
using Goldlight.Database.Models.v1;

namespace Goldlight.VirtualServer.Models.v1;

[DataContract]
public class ExtendedOrganization : Organization
{
  public ExtendedOrganization() { }
  public ExtendedOrganization(Organization organization)
  {
    Id = organization.Id;
    Name = organization.Name;
    Version = organization.Version;
  }

  [DataMember(Name="api-key")]
  public string? ApiKey { get; set; }

  public override OrganizationTable ToTable(int modelVersion = 1)
  {
    OrganizationTable table = base.ToTable(modelVersion);
    table.ApiKey = ApiKey;
    return table;
  }

  public static ExtendedOrganization FromTable(OrganizationTable table)
  {
    return new()
    {
      Id = table.Id,
      Name = table.Name,
      ApiKey = table.ApiKey,
      Version = table.Version ?? 0
    };
  }
}