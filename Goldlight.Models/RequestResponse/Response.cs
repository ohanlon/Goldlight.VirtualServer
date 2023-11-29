using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.Models.RequestResponse;

[DataContract]
public class Response
{
  [DataMember(Name = "responseid")] public Guid Id { get; set; } = Guid.NewGuid();

  [Required, DataMember(Name = "summary")]
  public HttpResponseSummary Summary { get; set; } = null!;

  [DataMember(Name = "headers")] public HttpHeader[]? Headers { get; set; }

  [DataMember(Name = "content")] public string? Content { get; set; }
}