using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.VirtualServer.Models.v1.RequestResponse;

[DataContract]
public class HttpResponseSummary
{
  [Required, DataMember(Name="version")] public string? Version { get; set; }
  [Required, DataMember(Name="status")] public int? Status { get; set; }
}