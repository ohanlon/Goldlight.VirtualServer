using System.Runtime.Serialization;
using Goldlight.Database.Models.v1;

namespace Goldlight.VirtualServer.Models.v1;

[DataContract]
public class Organization
{ 
  [DataMember(Name="id")]
  public string? Id { get; set; }

  [DataMember(Name="name")]
  public string? Name { get; set; }

  [DataMember(Name="version")]
  public long? Version { get; set; }


  public virtual OrganizationTable ToTable(int modelVersion = 1) {
    return new OrganizationTable
    {
      Id = Id!,
      Name = Name!,
      Version = Version,
      ModelVersion = modelVersion
    };
  }
}
