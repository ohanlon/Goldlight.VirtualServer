
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using Asp.Versioning.Builder;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Database.Models.v1.RequestResponse;

namespace Goldlight.VirtualServer.VirtualRequest;

public class VirtualRequestHandler
{
  private readonly RequestDelegate next;
  private const string X_API_KEY = "x-goldlight-api-key";

  public VirtualRequestHandler(RequestDelegate next)
  {
    this.next = next;
  }

  public async Task InvokeAsync(HttpContext context, OrganizationDataAccess organization, ProjectDataAccess project)
  {
    if (context.Request.Path.Value!.StartsWith("/api/"))
    {
      await next.Invoke(context);
      return;
    }

    string searchPath = context.Request.Path.Value + context.Request.QueryString;
    string[] apiDetails = searchPath.Split("/", StringSplitOptions.RemoveEmptyEntries);
    var organizationDetails = await organization.GetOrganizationAsync(apiDetails[0]);
    HttpResponse response = context.Response;
    if (organizationDetails is null)
    {
      response.StatusCode = (int)HttpStatusCode.NotFound;
      return;
    }

    string? apiKey = context.Request.Headers["x-goldlight-api-key"];
    if (string.IsNullOrWhiteSpace(apiKey) || apiKey != organizationDetails.ApiKey)
    {
      response.StatusCode = (int)StatusCodes.Status403Forbidden;
      return;
    }
    // This is where we would match the API Key passed in with the organization
    // using x-goldlight-api-key == organizationDetails.ApiKey
    var projectDetails = await project.GetProjectsAsync(organizationDetails.Id);
    var projectDetailsList = projectDetails.ToList();

    var projectItem = projectDetailsList.Find(item =>
      item.Details.FriendlyName.ToLowerInvariant() == apiDetails[1].ToLowerInvariant() &&
      item.Details.RequestResponsePairs is not null);
    if (projectItem is null)
    {
      await WriteResponse(context, $"** Service Virtualization Warning ** {context.Request.Path.Value + context.Request.QueryString} could not be found", 404);
      return;
    };
    int length = $"/{apiDetails[0]}/{apiDetails[1]}".Length;
    string actualApi = searchPath.Substring(length);
    await FindRequestResponsePairs(context, projectItem.Details.RequestResponsePairs!.Where(x => x.Request.Summary.Path.Equals(actualApi, StringComparison.InvariantCultureIgnoreCase)).ToList());
  }

  private async Task FindRequestResponsePairs(HttpContext context, List<RequestResponsePairTableFragment> rrpairs)
  {
    // Find the method
    List<RequestResponsePairTableFragment> possibles = rrpairs.FindAll(x => x.Request.Summary.Method == context.Request.Method);
    if (!possibles.Any())
    {
      await WriteResponse(context, $"** Service Virtualization Warning ** {context.Request.Path.Value + context.Request.QueryString} could not be found", 404);
      return;
    }

    foreach (var possible in possibles)
    {
      // Now, we find the matching entries.
      List<HttpHeaderTableFragment> headers = new();
      if (possible.Request.Headers is not null)
      {
        headers.AddRange(possible.Request.Headers);
      }

      if (!DoHeadersMatch(context.Request, headers))
      {
        continue;
      }
      string? content = possible.Request.Content;
      if (string.IsNullOrWhiteSpace(content))
      {
        content = string.Empty;
      }
      content = content.Trim();

      string body = await GetRawBodyAsync(context.Request);
      if (body != content)
      {
        continue;
      }
      // We have a match here. Let's write it out....
      if (possible.Response.Headers is not null)
      {
        foreach (var header in possible.Response.Headers)
        {
          context.Response.Headers!.TryAdd(header.Name, header.Value);
        }
      }
      await WriteResponse(context, possible.Response.Content, possible.Response.Summary.Status!.Value);
      break;
    }
  }

  static async Task WriteResponse(HttpContext context, string? result, int statusCode = 200, string contentType = "application/json")
  {
    HttpResponse response = context.Response;
    response.StatusCode = statusCode;
    response.ContentType = contentType;
    if (result is not null)
    {
      await response.WriteAsync(result);
    }
  }

  public static async Task<string> GetRawBodyAsync(HttpRequest request, Encoding? encoding = null)
  {
    if (!request.Body.CanSeek)
    {
      request.EnableBuffering();
    }

    request.Body.Position = 0;

    var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8);

    var body = await reader.ReadToEndAsync();//.ConfigureAwait(false);

    request.Body.Position = 0;

    return body;
  }

  private bool DoHeadersMatch(HttpRequest request, List<HttpHeaderTableFragment> headers)
  {
    foreach (var header in request.Headers)
    {
      if (header.Key.Equals(X_API_KEY, StringComparison.InvariantCultureIgnoreCase))
      {
        continue;
      }

      var headerItem = headers.Find(hdr => hdr.Name.Equals(header.Key, StringComparison.InvariantCultureIgnoreCase));
      if (headerItem is null) return false;
      headers.Remove(headerItem);
    }

    return headers.Count == 0;
  }
}
