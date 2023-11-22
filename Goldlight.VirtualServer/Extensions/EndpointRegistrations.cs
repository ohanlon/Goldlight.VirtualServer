using Asp.Versioning;

namespace Goldlight.VirtualServer.Extensions;

public static class EndpointRegistrations
{
  public static RouteGroupBuilder MapGroup(this WebApplication app, string versionsetName, ApiVersion version,
    string tag) =>
    app.MapGroup("/api").WithApiVersionSet(app.NewApiVersionSet(versionsetName).Build()).HasApiVersion(version)
      .RequireAuthorization().WithTags(tag);
}