using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.Serialization;

namespace Goldlight.Models;

[DataContract]
public class Organization
{
  private string friendlyName = null!;
  [DataMember(Name = "id")] public Guid Id { get; set; } = Guid.Empty;

  [DataMember(Name = "name"), MaxLength(120)]
  public string Name { get; set; } = null!;

  [DataMember(Name = "friendlyname"), MaxLength(120)]
  public string FriendlyName
  {
    get => WebUtility.UrlEncode(friendlyName);
    set => friendlyName = value;
  }

  [DataMember(Name = "version")] public long Version { get; set; }

  [DataMember(Name = "api-key")] public string ApiKey { get; set; }
}