using Asp.Versioning;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Database.Exceptions;
using Goldlight.Database.Models.v1;
using Goldlight.Models;
using Goldlight.VirtualServer.Models;
using Goldlight.VirtualServer.Models.v1;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Goldlight.VirtualServer.Extensions;

public static class EndpointRegistrations
{
  public static void RegisterUserEndpoints(this WebApplication app, ApiVersion version)
  {
    RouteGroupBuilder mapGroup = app.MapGroup("Users", version, "User operations");

    mapGroup.MapPost("/api/user", async (UserDataAccess dataAccess, HttpContext context) =>
    {
      dataAccess.AddUser(context.EmailAddress());
      return TypedResults.Ok();
    });
  }

  public static void RegisterProjectEndpoints(this WebApplication app, ApiVersion version)
  {
    RouteGroupBuilder mapGroup = app.MapGroup("Projects", version, "Projects operations");

    //mapGroup.MapPost("/api/project", async (ProjectDataAccess dataAccess, ProjectTable project) =>
    //{
    //  ExtendedProject extendedProject = new(project)
    //  {
    //    Id = Guid.NewGuid()
    //  };
    //  await dataAccess.SaveProjectAsync(extendedProject.ToTable());
    //  extendedProject.Version = 0;
    //  return TypedResults.Created($"/api/organization/{extendedProject.Id}", extendedProject);
    //});

    mapGroup.MapPut("/api/project", async (ProjectDataAccess dataAccess, ExtendedProject project) =>
    {
      await dataAccess.SaveProjectAsync(project.ToTable());
      project.Version++;
      return TypedResults.Ok(project);
    });

    mapGroup.MapGet("/api/{organization}/projects/", async (ProjectDataAccess dataAccess, string organization) =>
    {
      IEnumerable<ProjectTable> allProjects = await dataAccess.GetProjectsAsync(organization);
      return allProjects.Select(ExtendedProject.FromTable);
    });

    mapGroup.MapDelete("/api/project/{id}", async (ProjectDataAccess dataAccess, string id) =>
    {
      await dataAccess.DeleteProjectAsync(id);
      return TypedResults.Ok(id);
    });
  }

  public static void RegisterOrganizationEndpoints(this WebApplication app, ApiVersion version)
  {
    RouteGroupBuilder mapGroup = app.MapGroup("Organizations", version, "Organizations operations");

    mapGroup.MapPost("/organization", CreateOrganization);
    mapGroup.MapGet("/organization/{id}", GetOrganizationById);
    mapGroup.MapGet("/organization/name/{name}", GetOrganizationByName);
    mapGroup.MapGet("/organizations", GetOrganizations);
  }

  private static async Task<Results<Created<Organization>, Ok<Organization>, Conflict>>
    CreateOrganization(OrganizationDataAccess dataAccess, Organization organization, HttpContext context) =>
    await AddOrganization(organization, dataAccess, context.EmailAddress()) is { } org
      ? TypedResults.Created($"/organization/{org.Id}", org)
      : TypedResults.Conflict();

  private static async Task<Results<Ok<IEnumerable<Organization>>, NotFound>> GetOrganizations(
    OrganizationDataAccess dataAccess, HttpContext context) =>
    await dataAccess.GetOrganizationsAsync(context.EmailAddress()) is { } organizations
      ? TypedResults.Ok(organizations)
      : TypedResults.NotFound();

  private static async Task<Results<Ok<Organization>, NotFound, UnauthorizedHttpResult>> GetOrganizationById(
    OrganizationDataAccess oda,
    Guid id, HttpContext context) =>
    await oda.IsUserPresentInOrganization(id, context.EmailAddress())
      ? TypedResults.Unauthorized()
      : await oda.GetOrganizationAsync(id) is { } organization
        ? TypedResults.Ok(organization)
        : TypedResults.NotFound();

  private static async Task<Results<Ok<Organization>, NotFound, UnauthorizedHttpResult>> GetOrganizationByName(
    OrganizationDataAccess oda,
    string name, HttpContext context) =>
    await oda.IsUserPresentInOrganization(name, context.EmailAddress())
      ? TypedResults.Unauthorized()
      : await oda.GetOrganizationByNameAsync(name) is { } organization
        ? TypedResults.Ok(organization)
        : TypedResults.NotFound();

  private static async Task<Organization?> AddOrganization(Organization organization, OrganizationDataAccess dataAccess,
    string emailAddress)
  {
    try
    {
      organization.Id = Guid.NewGuid();
      organization.ApiKey = Guid.NewGuid().ToString().Replace("-", "");
      organization.Version = 0;
      await dataAccess.SaveAsync(organization, emailAddress);
    }
    catch (SaveConflictException)
    {
      return null;
    }

    return organization;
  }

  private static RouteGroupBuilder MapGroup(this WebApplication app, string versionsetName, ApiVersion version,
    string tag) =>
    app.MapGroup("/api").WithApiVersionSet(app.NewApiVersionSet(versionsetName).Build()).HasApiVersion(version)
      .RequireAuthorization().WithTags(tag);
}