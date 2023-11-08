using Asp.Versioning;
using Asp.Versioning.Builder;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Database.Exceptions;
using Goldlight.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Goldlight.VirtualServer.Extensions;

public static class EndpointRegistrations
{
  public static void RegisterOrganizationEndpoints(this WebApplication app, ApiVersion version)
  {
    ApiVersionSet organizations = app.NewApiVersionSet("Organizations").Build();
    RouteGroupBuilder mapGroup = app.MapGroup("/api").WithApiVersionSet(organizations).HasApiVersion(version)
      .RequireAuthorization().WithTags("Organization operations");

    mapGroup.MapPost("/organization", CreateOrganization);
    mapGroup.MapGet("/organization/{id}", GetOrganizationById);
    mapGroup.MapGet("/organization/name/{name}", GetOrganizationByName);
    mapGroup.MapGet("/organizations",
      async (OrganizationDataAccess dataAccess, HttpContext context) =>
      {
        return await dataAccess.GetOrganizationsAsync(context.EmailAddress());
      });
    ;

    //mapGroup.MapPut("/organization", async (OrganizationDataAccess dataAccess, Organization organization) =>
    //{
    //  await dataAccess.SaveAsync(organization.ToTable());
    //  organization.Version++;
    //  return TypedResults.Ok(organization);
    //});

    //mapGroup.MapDelete("/organization/{id}", async (OrganizationDataAccess dataAccess, string id) =>
    //{
    //  await dataAccess.DeleteOrganizationAsync(id);
    //  return TypedResults.Ok(id);
    //});
  }

  private static async Task<Results<Created<Organization>, Ok<Organization>, Conflict>>
    CreateOrganization(OrganizationDataAccess dataAccess, Organization organization, HttpContext context) =>
    await AddOrganization(organization, dataAccess, context.EmailAddress()) is { } org
      ? TypedResults.Created($"/organization/{org.Id}", org)
      : TypedResults.Conflict();

  private static async Task<Results<Ok<Organization>, NotFound>> GetOrganizationById(OrganizationDataAccess oda,
    Guid id) =>
    await oda.GetOrganizationAsync(id) is { } organization
      ? TypedResults.Ok(organization)
      : TypedResults.NotFound();

  private static async Task<Results<Ok<Organization>, NotFound>> GetOrganizationByName(OrganizationDataAccess oda,
    string name) =>
    await oda.GetOrganizationByNameAsync(name) is { } organization
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
}