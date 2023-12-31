using System.Net;
using System.Runtime.Serialization;
using Goldlight.Database.Models.v1;

namespace Goldlight.VirtualServer.Models.v1;

[DataContract]
public class ExtendedOrganization : Organization
{
  public ExtendedOrganization() { }
  public ExtendedOrganization(Organization organization)
  {
    Id = WebUtility.UrlEncode(organization.Id);
    Name = organization.Name;
    Version = organization.Version;
  }

  [DataMember(Name="api-key")]
  public string? ApiKey { get; set; }

  public static ExtendedOrganization FromTable(OrganizationTable table)
  {
    return new ExtendedOrganization
    {
      Id = table.Id,
      Name = table.Name,
      ApiKey = table.ApiKey,
      Version = table.Version ?? 0,
    };
  }
}