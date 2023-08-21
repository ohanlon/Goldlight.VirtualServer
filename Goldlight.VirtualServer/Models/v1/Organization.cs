using System.Runtime.Serialization;
using Goldlight.Database.Models.v1;

namespace Goldlight.VirtualServer.Models.v1;

[DataContract]
public class Organization
{ 
  [DataMember(Name="id")]
  public Guid? Id { get; set; }

  [DataMember(Name="name")]
  public string? Name { get; set; }

  [DataMember(Name="version")]
  public long? Version { get; set; }

  public static Organization FromTable(OrganizationTable table)
  {
    return new()
    {
      Id = Guid.Parse(table.Id!),
      Name = table.Name,
      Version = table.Version ?? 0
    };
  }

  public OrganizationTable ToTable(int modelVersion = 1) {
    return new()
    {
      Id = Id.ToString(),
      Name = Name,
      Version = Version,
      ModelVersion = modelVersion
    };
  }
}
