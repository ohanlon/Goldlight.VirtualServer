using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.Models.RequestResponse;

[DataContract]
public class RequestResponsePair
{
  [DataMember(Name = "id")] public Guid Id { get; set; } = Guid.NewGuid();
  [DataMember(Name = "projectid")] public Guid ProjectId { get; set; }

  [Required, DataMember(Name = "name"), MinLength(10), MaxLength(120)]
  public string Name { get; set; } = null!;

  [Required, DataMember(Name = "description"), MinLength(10), MaxLength(500)]
  public string Description { get; set; } = null!;

  [Required, DataMember(Name = "request")]
  public Request Request { get; set; } = new();

  [Required, DataMember(Name = "response")]
  public Response Response { get; set; } = new();

  [DataMember(Name = "version")] public long Version { get; set; }
}