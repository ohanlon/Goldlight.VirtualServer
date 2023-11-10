using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.Models;

[DataContract]
public class Project
{
  [DataMember(Name = "id")] public Guid Id { get; set; } = Guid.Empty;

  [Required, DataMember(Name = "organization")]
  public Guid Organization { get; set; } = Guid.Empty;

  [Required, DataMember(Name = "name"), MaxLength(120)]
  public string Name { get; set; } = null!;

  [Required, DataMember(Name = "friendlyname"), MaxLength(120)]
  public string FriendlyName { get; set; } = null!;

  [Required, DataMember(Name = "description")]
  public string? Description { get; set; }

  [DataMember(Name = "version")] public long Version { get; set; }
}