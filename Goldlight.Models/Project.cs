using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Goldlight.Models;

[DataContract]
public class Project
{
  [Column("id"), DataMember(Name = "id")]
  public Guid Id { get; set; } = Guid.Empty;

  [Required, Column("organization_id"), DataMember(Name = "organization")]
  public Guid Organization { get; set; } = Guid.Empty;

  [Required, Column("name"), DataMember(Name = "name"), MaxLength(120)]
  public string Name { get; set; } = null!;

  [Required, Column("friendlyname"), DataMember(Name = "friendlyname"), MaxLength(120)]
  public string FriendlyName { get; set; } = null!;

  [Required, Column("description"), DataMember(Name = "description")]
  public string? Description { get; set; }

  [Column("version"), DataMember(Name = "version")]
  public long Version { get; set; }
}