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

    mapGroup.MapPost("user", async (UserDataAccess dataAccess, HttpContext context) =>
    {
      await dataAccess.AddUser(context.EmailAddress());
      return TypedResults.Ok();
    });

    mapGroup.MapDelete("{organization}/user/{emailAddress}",
      async (UserDataAccess dataAccess, HttpContext context, Guid organization, string emailAddress) =>
      {
        await CheckUserCanEdit(dataAccess, organization, context.EmailAddress());
        await dataAccess.DeleteUserFromOrganization(emailAddress, organization);
        return TypedResults.Ok("User deleted");
      });

    mapGroup.MapGet("{organization}/user/canedit",
      async (UserDataAccess dataAccess, HttpContext context, Guid organization) =>
        TypedResults.Ok(await EnsureUserHasEditCapability(dataAccess, organization, context.EmailAddress())));
  }

  public static async Task CheckUserCanEdit(this UserDataAccess userDataAccess, Project project, HttpContext context) =>
    await CheckUserCanEdit(userDataAccess, project.Organization, context.EmailAddress());

  public static async Task CheckUserCanEdit(this UserDataAccess userDataAccess, Guid organization,
    string emailAddress)
  {
    if (await EnsureUserHasEditCapability(userDataAccess, organization, emailAddress)) return;

    throw new ForbiddenException();
  }

  private static async Task<bool> EnsureUserHasEditCapability(UserDataAccess userDataAccess, Guid organization,
    string emailAddress)
  {
    await CheckUserHasAccess(userDataAccess, emailAddress, organization);
    string? role = await userDataAccess.UserRoleForOrganization(emailAddress, organization);
    return role is "PRIMARY OWNER" or "OWNER" or "EDITOR";
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