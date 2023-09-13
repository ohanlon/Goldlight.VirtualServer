using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.Serialization;

namespace Goldlight.VirtualServer.Models.v1;

public class Project
{
  [Required]
  public string? Name { get; set; }
  [Required]
  public string? FriendlyName { get; set; }
  [Required]
  public string? Description { get; set; }
  [Required]
  public string Organization { get; set; }

  [DataMember(Name = "version")] public long? Version { get; set; }
}