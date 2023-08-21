using System.Runtime.Serialization;

namespace Goldlight.VirtualServer.Models.v1;

/// <summary>
/// When organizations or individuals sign up to use Service Virtualization, their services will all be deployed against that organization.
/// </summary>
[DataContract]
public class Organization
{ 
  /// <summary>
  /// The &#x27;primary key&#x27; of the organization.
  /// </summary>
  /// <value>The &#x27;primary key&#x27; of the organization.</value>
  [DataMember(Name="id")]
  public Guid? Id { get; set; }

  /// <summary>
  /// The name of the organization.
  /// </summary>
  /// <value>The name of the organization.</value>
  [DataMember(Name="name")]
  public string? Name { get; set; }

}

[DataContract]
public class OrganizationResponse : Organization
{
  public OrganizationResponse(Organization organization)
  {
    Id = organization.Id;
    Name = organization.Name;
  }
  [DataMember(Name = "lastModified")]
  public DateTime? LastModified { get; set; } = DateTime.UtcNow;
}