﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Goldlight.Models.RequestResponse;

[DataContract]
public class Request
{
  [DataMember(Name = "requestid")] public Guid Id { get; set; } = Guid.NewGuid();

  [Required, DataMember(Name = "summary")]
  public HttpRequestSummary Summary { get; set; } = null!;

  [DataMember(Name = "headers")] public HttpHeader[]? Headers { get; set; }

  [DataMember(Name = "requestcontent")] public string? Content { get; set; }
}