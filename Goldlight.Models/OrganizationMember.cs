using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.Models;

[DataContract]
public class OrganizationMember
{
  [DataMember(Name = "userid"), Required, MinLength(4), MaxLength(1024)]
  public string EmailAddress { get; set; }

  [DataMember(Name = "rolename"), Required, MaxLength(20)]
  public string Role { get; set; }
}