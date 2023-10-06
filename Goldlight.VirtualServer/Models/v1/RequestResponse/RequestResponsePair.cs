using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.VirtualServer.Models.v1.RequestResponse;

[DataContract]
public class RequestResponsePair
{
  [Required, DataMember(Name="name"), MinLength(10)] public string Name { get; set; } = null!;
  [Required, DataMember(Name="description"), MinLength(10)] public string Description { get; set; } = null!;
  [Required, DataMember(Name="request")] public Request Request { get; set; } = null!;
  [Required, DataMember(Name="response")] public Response Response { get; set; } = null!;
}