using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.VirtualServer.Models.v1.RequestResponse;

[DataContract]
public class Request
{
  [Required, DataMember(Name = "summary")]
  public HttpRequestSummary Summary { get; set; } = null!;

  [DataMember(Name="headers")]
  public HttpHeader[]? Headers { get; set; }

  [DataMember(Name="content")]
  public string? Content { get; set; }
}