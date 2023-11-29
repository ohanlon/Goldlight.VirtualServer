using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.Models.RequestResponse;

[DataContract]
public class HttpRequestSummary
{
  [DataMember(Name = "requestsummaryid")]
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required, DataMember(Name = "method"), MinLength(1), MaxLength(10)]
  public string Method { get; set; } = "ANY";

  [Required, DataMember(Name = "path"), MinLength(1), MaxLength(120)]
  public string? Path { get; set; }

  [DataMember(Name = "protocol"), MinLength(1), MaxLength(64)]
  public string? Protocol { get; set; }
}