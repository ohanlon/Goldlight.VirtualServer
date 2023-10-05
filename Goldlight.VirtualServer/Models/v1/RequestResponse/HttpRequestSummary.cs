using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.VirtualServer.Models.v1.RequestResponse;

[DataContract]
public class HttpRequestSummary
{
  [Required, DataMember(Name = "method")]
  public string Method { get; set; } = null!;
  [Required, DataMember(Name = "path")] public string Path { get; set; } = null!;
  [DataMember(Name="version")] public string? Version { get; set; }
}