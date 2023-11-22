using Asp.Versioning;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Database.Exceptions;
using Goldlight.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Goldlight.VirtualServer.Extensions;

public static class OrganizationRegistrations
{
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
    await SaveOrganization(organization, dataAccess, context.EmailAddress()) is { } org
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

  private static async Task<Organization?> SaveOrganization(Organization organization,
    OrganizationDataAccess dataAccess,
    string emailAddress)
  {
    try
    {
      if (organization.Id == Guid.Empty)
      {
        organization.Id = Guid.NewGuid();
        organization.ApiKey = Guid.NewGuid().ToString().Replace("-", "");
      }

      await dataAccess.SaveAsync(organization, emailAddress);
    }
    catch (SaveConflictException)
    {
      return null;
    }

    return organization;
  }
}