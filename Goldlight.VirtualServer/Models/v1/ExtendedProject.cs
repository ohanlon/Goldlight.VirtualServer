﻿using System.Runtime.Serialization;
using Goldlight.Database.Models.v1;
using Goldlight.Database.Models.v1.RequestResponse;
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

  [DataMember]
  public Guid Id { get; set; }

  [DataMember(Name = "requestResponses")] public RequestResponsePair[] RequestResponses { get; set; }

  public virtual ProjectTable ToTable(int modelVersion = 1)
  {
    List<RequestResponsePairTableFragment> rrPairs = new();
    foreach (RequestResponsePair requestResponse in RequestResponses)
    {
      HttpHeaderTableFragment[]? requestHeaders = null;
      if (requestResponse.Request.Headers is not null)
      {
        requestResponse.Request.Headers.Select(requestHeader => new HttpHeaderTableFragment { Name = requestHeader.Name, Value = requestHeader.Value }).ToArray();
      }
      HttpHeaderTableFragment[]? responseHeaders = null;
      if (requestResponse.Response.Headers is not null)
      {
        requestResponse.Response.Headers.Select(responseHeader => new HttpHeaderTableFragment
          { Name = responseHeader.Name, Value = responseHeader.Value }).ToArray();
      }

      RequestResponsePairTableFragment fragment = new RequestResponsePairTableFragment
      {
        Description = requestResponse.Description,
        Name = requestResponse.Name,
        Response = new()
        {
          Content = requestResponse.Response.Content,
          Headers = responseHeaders,
          Summary = new()
          {
            Status = requestResponse.Response.Summary.Status,
            Version = requestResponse.Response.Summary.Version
          }
        },
        Request = new()
        {
          Content = requestResponse.Request.Content,
          Headers = requestHeaders,
          Summary = new()
          {
            Version = requestResponse.Request.Summary.Version,
            Method = requestResponse.Request.Summary.Method,
            Path = requestResponse.Request.Summary.Path
          }
        }
      };
      rrPairs.Add(fragment);
    }
    ProjectTable projectTable = new()
    {
      Id = Id.ToString(),
      OrganizationId = Organization,
      Details = new()
      {
        Description = Description,
        FriendlyName = FriendlyName,
        Name = Name,
      },
      ModelVersion = modelVersion,
      Version = Version
    };
    if (rrPairs.Any())
    {
      projectTable.Details.RequestResponsePairs = rrPairs.ToArray();
    }
    return projectTable;
  }

  public static ExtendedProject FromTable(ProjectTable table)
  {
    ExtendedProject project = new ExtendedProject
    {
      Id = Guid.Parse(table.Id),
      Organization = table.OrganizationId,
      Name = table.Details.Name,
      Description = table.Details.Description,
      FriendlyName = table.Details.FriendlyName,
      Version = table.Version
    };
    if (table.Details.RequestResponsePairs == null) return project;
    var requestResponses = new List<RequestResponsePair>();
    
    foreach (var detailsRequestResponsePair in table.Details.RequestResponsePairs)
    {
      RequestResponsePair rrPair = new();
      List<HttpHeader> requestHeaders = new List<HttpHeader>();
      if (detailsRequestResponsePair.Request.Headers is not null)
      {
        requestHeaders.AddRange(detailsRequestResponsePair.Request.Headers.Select(header =>
          new HttpHeader { Name = header.Name, Value = header.Value }));
      }

      List<HttpHeader> responseHeaders = new List<HttpHeader>();
      if (detailsRequestResponsePair.Response.Headers is not null)
      {
        responseHeaders.AddRange(detailsRequestResponsePair.Response.Headers.Select(header =>
          new HttpHeader { Name = header.Name, Value = header.Value }));
      }

      rrPair.Description = detailsRequestResponsePair.Description;
      rrPair.Name = detailsRequestResponsePair.Name;
      rrPair.Request = new()
      {
        Content = detailsRequestResponsePair.Request.Content,
        Headers = requestHeaders.ToArray(),
        Summary = new()
        {
          Version = detailsRequestResponsePair.Request.Summary.Version,
          Method = detailsRequestResponsePair.Request.Summary.Method,
          Path = detailsRequestResponsePair.Request.Summary.Path
        }
      };
      rrPair.Response = new()
      {
        Summary = new()
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