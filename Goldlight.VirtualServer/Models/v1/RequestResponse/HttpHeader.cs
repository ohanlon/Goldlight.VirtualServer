using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.VirtualServer.Models.v1.RequestResponse;

[DataContract]
public class HttpHeader
{
  [Required, DataMember(Name = "name"), MinLength(1)]
  public string? Name { get; set; }

  [Required, DataMember(Name = "value"), MinLength(1)]
  public string? Value { get; set; }
}