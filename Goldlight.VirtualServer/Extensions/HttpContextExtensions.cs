using System.Security.Claims;

namespace Goldlight.VirtualServer.Extensions;

public static class HttpContextExtensions
{
  public static string EmailAddress(this HttpContext context) =>
    context.User.Claims.Single(x => x.Type == ClaimTypes.Email).Value;
}