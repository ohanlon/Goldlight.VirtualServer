using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Goldlight.Models;

[DataContract]
public class Project
{
  [Column("id"), DataMember(Name = "id")]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required, DataMember(Name = "organization_id")]
  public Guid Organization { get; set; } = Guid.Empty;

  [Required, DataMember(Name = "name"), MaxLength(120)]
  public string Name { get; set; } = null!;

  [Required, DataMember(Name = "friendlyname"), MaxLength(120)]
  public string FriendlyName { get; set; } = null!;

  [Required, DataMember(Name = "description")]
  public string? Description { get; set; }

  [Required, DataMember(Name = "version")]
  public long Version { get; set; }
}