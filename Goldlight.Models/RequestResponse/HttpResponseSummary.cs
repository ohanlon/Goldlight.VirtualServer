using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.Models.RequestResponse;

[DataContract]
public class HttpResponseSummary
{
  [DataMember(Name = "responsesummaryid")]
  public Guid Id { get; set; } = Guid.NewGuid();

  [DataMember(Name = "responseprotocol"), Required, MinLength(1), MaxLength(32)]
  public string? Protocol { get; set; }

  [DataMember(Name = "status"), Required]
  public int? Status { get; set; }
}