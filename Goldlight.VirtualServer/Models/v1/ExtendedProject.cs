using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Goldlight.Database.Models.v1;
using Goldlight.VirtualServer.Models.v1.RequestResponse;
using HttpResponseSummary = Goldlight.VirtualServer.Models.v1.RequestResponse.HttpResponseSummary;
using RequestResponsePair = Goldlight.VirtualServer.Models.v1.RequestResponse.RequestResponsePair;

namespace Goldlight.VirtualServer.Models.v1;

public class ExtendedProject : Project
{
  public ExtendedProject()
  {
  }

  public ExtendedProject(Project project)
  {
    Organization = project.Organization;
    Name = project.Name;
    Description = project.Description;
    FriendlyName = project.FriendlyName;
  }

  [DataMember, Required]
  public Guid Id { get; set; }

  [DataMember(Name = "requestResponses")] public RequestResponsePair[]? RequestResponses { get; set; }

  public static ExtendedProject FromTable(ProjectTable table)
  {
    ExtendedProject project = new()
    {
      Id = Guid.Parse(table.Id),
      Organization = table.OrganizationId,
      Name = table.Details.Name,
      Description = table.Details.Description,
      FriendlyName = table.Details.FriendlyName,
      Version = table.Version
    };
    if (table.Details.RequestResponsePairs == null) return project;
    List<RequestResponsePair> requestResponses = new();
    
    foreach (var detailsRequestResponsePair in table.Details.RequestResponsePairs)
    {
      RequestResponsePair rrPair = new();
      List<HttpHeader> requestHeaders = new();
      if (detailsRequestResponsePair.Request.Headers is not null)
      {
        requestHeaders.AddRange(detailsRequestResponsePair.Request.Headers.Select(header =>
          new HttpHeader { Name = header.Name, Value = header.Value }));
      }

      List<HttpHeader> responseHeaders = new();
      if (detailsRequestResponsePair.Response.Headers is not null)
      {
        responseHeaders.AddRange(detailsRequestResponsePair.Response.Headers.Select(header =>
          new HttpHeader { Name = header.Name, Value = header.Value }));
      }

      rrPair.Description = detailsRequestResponsePair.Description;
      rrPair.Name = detailsRequestResponsePair.Name;
      rrPair.Request = new Request
      {
        Content = detailsRequestResponsePair.Request.Content,
        Headers = requestHeaders.ToArray(),
        Summary = new HttpRequestSummary
        {
          Version = detailsRequestResponsePair.Request.Summary.Version,
          Method = detailsRequestResponsePair.Request.Summary.Method,
          Path = detailsRequestResponsePair.Request.Summary.Path
        }
      };
      rrPair.Response = new Response
      {
        Summary = new HttpResponseSummary
        {
          Status = detailsRequestResponsePair.Response.Summary.Status,
          Version = detailsRequestResponsePair.Response.Summary.Version
        },
        Headers = responseHeaders.ToArray()
      };
      requestResponses.Add(rrPair);
    }

    project.RequestResponses = requestResponses.ToArray();

    return project;
  }
}