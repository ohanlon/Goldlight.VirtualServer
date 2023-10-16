using Goldlight.Database.Models.v1.RequestResponse;
using Goldlight.Database.Models.v1;
using Goldlight.VirtualServer.Models.v1;

namespace Goldlight.VirtualServer.Models;

public static class ModelExtensions
{
  public static ProjectTable ToTable(this ExtendedProject project, int modelVersion = 1)
  {
    List<RequestResponsePairTableFragment> rrPairs = new();
    if (project.RequestResponses is not null)
    {
      BuildRestResponsePairsForProject(project, rrPairs);
    }

    ProjectTable projectTable = new()
    {
      Id = project.Id.ToString(),
      OrganizationId = project.Organization,
      Details = new Details
      {
        Description = project.Description,
        FriendlyName = project.FriendlyName,
        Name = project.Name,
      },
      ModelVersion = modelVersion,
      Version = project.Version
    };
    if (rrPairs.Any())
    {
      projectTable.Details.RequestResponsePairs = rrPairs.ToArray();
    }
    return projectTable;
  }

  private static void BuildRestResponsePairsForProject(ExtendedProject project, List<RequestResponsePairTableFragment> rrPairs)
  {
    foreach (var requestResponse in project.RequestResponses)
    {
      HttpHeaderTableFragment[]? requestHeaders = null;
      if (requestResponse.Request.Headers is not null)
      {
        requestHeaders = requestResponse.Request.Headers.Select(requestHeader => new HttpHeaderTableFragment
          { Name = requestHeader.Name, Value = requestHeader.Value }).ToArray();
      }

      HttpHeaderTableFragment[]? responseHeaders = null;
      if (requestResponse.Response.Headers is not null)
      {
        responseHeaders = requestResponse.Response.Headers.Select(responseHeader => new HttpHeaderTableFragment
          { Name = responseHeader.Name, Value = responseHeader.Value }).ToArray();
      }

      RequestResponsePairTableFragment fragment = new()
      {
        Description = requestResponse.Description,
        Name = requestResponse.Name,
        Response = new ResponseTableFragment
        {
          Content = requestResponse.Response.Content,
          Headers = responseHeaders,
          Summary = new HttpResponseSummaryTableFragment
          {
            Status = requestResponse.Response.Summary.Status,
            Protocol = requestResponse.Response.Summary.Protocol,
          }
        },
        Request = new RequestTableFragment
        {
          Content = requestResponse.Request.Content,
          Headers = requestHeaders,
          Summary = new Summary
          {
            Protocol = requestResponse.Request.Summary.Protocol,
            Method = requestResponse.Request.Summary.Method,
            Path = requestResponse.Request.Summary.Path
          }
        }
      };
      rrPairs.Add(fragment);
    }
  }

  public static OrganizationTable ToTable(this ExtendedOrganization organization, int modelVersion = 1)
  {
    OrganizationTable table = new()
    {
      Id = organization.Id!,
      Name = organization.Name!,
      Version = organization.Version,
      ModelVersion = modelVersion,
      ApiKey = organization.ApiKey
    };
    return table;
  }

  public static OrganizationTable ToTable(this Organization organization, int modelVersion = 1)
  {
    return new OrganizationTable
    {
      Id = organization.Id!,
      Name = organization.Name!,
      Version = organization.Version,
      ModelVersion = modelVersion
    };
  }
}
