using Asp.Versioning;
using Goldlight.Database.DatabaseOperations;

namespace Goldlight.VirtualServer.Extensions;

public static class UserRegistrations
{
  public static void RegisterUserEndpoints(this WebApplication app, ApiVersion version)
  {
    RouteGroupBuilder mapGroup = app.MapGroup("Users", version, "User operations");

    mapGroup.MapPost("/api/user", async (UserDataAccess dataAccess, HttpContext context) =>
    {
      await dataAccess.AddUser(context.EmailAddress());
      return TypedResults.Ok();
    });
  }
}