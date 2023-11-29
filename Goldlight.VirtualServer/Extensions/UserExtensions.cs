using Asp.Versioning;
using Goldlight.Database.DatabaseOperations;
using Goldlight.ExceptionManagement;

namespace Goldlight.VirtualServer.Extensions;

public static class UserExtensions
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

  public static async Task CheckUserCanEdit(this UserDataAccess userDataAccess, Guid projectId, string emailAddress)
  {
    string? role = await userDataAccess.UserRoleForProject(emailAddress, projectId);
    if (role is not null && role is "PRIMARY OWNER" or "OWNER" or "EDITOR")
    {
      return;
    }

    throw new ForbiddenException();
  }
}