using Asp.Versioning;
using Goldlight.Database.DatabaseOperations;
using Goldlight.ExceptionManagement;
using Goldlight.Models;

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

  public static async Task CheckUserCanEdit(this UserDataAccess userDataAccess, Project project, HttpContext context) =>
    await CheckUserCanEdit(userDataAccess, project.Organization, context.EmailAddress());

  public static async Task CheckUserCanEdit(this UserDataAccess userDataAccess, Guid organization,
    string emailAddress)
  {
    await CheckUserHasAccess(userDataAccess, emailAddress, organization);
    string? role = await userDataAccess.UserRoleForProject(emailAddress, organization);
    if (role is not null && role is "PRIMARY OWNER" or "OWNER" or "EDITOR")
    {
      return;
    }

    throw new ForbiddenException();
  }

  public static async Task CheckUserHasAccess(this UserDataAccess userDataAccess, string emailAddress,
    Guid organization)
  {
    bool hasAccess = await userDataAccess.UserInOrganization(emailAddress, organization);
    if (!hasAccess)
    {
      throw new ForbiddenException();
    }
  }
}