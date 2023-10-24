using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.VirtualServer.Models.v1;

public class Project
{
  [Required, DataMember] public string Name { get; set; } = null!;
  [Required, DataMember] public string FriendlyName { get; set; } = null!;
  [Required, DataMember] public string Description { get; set; } = null!;
  [Required, DataMember] public string Organization { get; set; } = null!;

  [DataMember(Name = "version")] public long? Version { get; set; }
}