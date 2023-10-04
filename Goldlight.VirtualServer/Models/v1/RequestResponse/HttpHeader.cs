using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.VirtualServer.Models.v1.RequestResponse;

[DataContract]
public class HttpHeader
{
  [Required, DataMember(Name="name")] public string? Name { get; set; }
  [Required, DataMember(Name="value")] public string? Value { get; set; }
}