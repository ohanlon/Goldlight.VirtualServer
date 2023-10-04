using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.VirtualServer.Models.v1.RequestResponse;

[DataContract]
public class Response
{
  [Required, DataMember(Name = "summary")] public HttpResponseSummary? Summary { get; set; }
  [DataMember(Name="headers")]
  public HttpHeader[]? Headers { get; set; }

  [DataMember(Name="content")]
  public string? Content { get; set; }
}