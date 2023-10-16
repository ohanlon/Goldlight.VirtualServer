using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.VirtualServer.Models.v1.RequestResponse;

[DataContract]
public class HttpResponseSummary
{
  [Required, DataMember(Name="protocol")] public string? Protocol { get; set; }
  [Required, DataMember(Name="status")] public int? Status { get; set; }
}