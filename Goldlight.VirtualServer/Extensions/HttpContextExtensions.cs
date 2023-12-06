using System.Security.Claims;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Models;

namespace Goldlight.VirtualServer.Extensions;

public static class HttpContextExtensions
{
  public static string EmailAddress(this HttpContext context) =>
    context.User.Claims.Single(x => x.Type == ClaimTypes.Email).Value;

  public static async Task CheckUserHasAccess(this HttpContext context, UserDataAccess userDataAccess,
    Guid organization) =>
    await userDataAccess.CheckUserHasAccess(context.EmailAddress(), organization);

  public static async Task CheckUserCanEdit(this HttpContext context, UserDataAccess userDataAccess, Project project) =>
    await userDataAccess.CheckUserCanEdit(project, context);

  public static async Task CheckUserCanEdit(this HttpContext context, UserDataAccess userDataAccess, Guid organization,
    Guid project) =>
    await userDataAccess.CheckUserCanEdit(organization, project, context.EmailAddress());
}