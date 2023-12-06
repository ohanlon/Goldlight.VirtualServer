﻿using Asp.Versioning;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Goldlight.VirtualServer.Extensions;

public static class OrganizationExtensions
{
  public static void RegisterOrganizationEndpoints(this WebApplication app, ApiVersion version)
  {
    RouteGroupBuilder mapGroup = app.MapGroup("Organizations", version, "Organizations operations");

    mapGroup.MapPost("/organization", CreateOrganization);
    mapGroup.MapGet("/organization/{id}", GetOrganizationById);
    mapGroup.MapGet("/organization/name/{name}", GetOrganizationByName);
    mapGroup.MapGet("/organizations", GetOrganizations);
  }

  private static async Task<Results<Created<Organization>, Ok<Organization>>>
    CreateOrganization(OrganizationDataAccess dataAccess, Organization organization, HttpContext context) =>
    await SaveOrganization(organization, dataAccess, context.EmailAddress()) is { Version: > 0 } org
      ? TypedResults.Created($"/organization/{org.Id}", org)
      : TypedResults.Ok(organization);

  private static async Task<Results<Ok<IEnumerable<Organization>>, NotFound>> GetOrganizations(
    OrganizationDataAccess dataAccess, HttpContext context) =>
    await dataAccess.GetOrganizationsAsync(context.EmailAddress()) is { } organizations
      ? TypedResults.Ok(organizations)
      : TypedResults.NotFound();

  private static async Task<Results<Ok<Organization>, NotFound>> GetOrganizationById(
    OrganizationDataAccess oda,
    Guid id, HttpContext context)
  {
    await oda.ValidateUserIsPresentInOrganization(id, context.EmailAddress());
    var organization = await oda.GetOrganizationAsync(id);
    if (organization is null)
    {
      return TypedResults.NotFound();
    }

    return TypedResults.Ok(organization);
  }

  private static async Task<Results<Ok<Organization>, NotFound>> GetOrganizationByName(
    OrganizationDataAccess oda,
    string name, HttpContext context)
  {
    await oda.ValidateUserIsPresentInOrganization(name, context.EmailAddress());
    var organization = await oda.GetOrganizationByNameAsync(name);
    if (organization is null)
    {
      return TypedResults.NotFound();
    }

    return TypedResults.Ok(organization);
  }

  private static async Task<Organization> SaveOrganization(Organization organization,
    OrganizationDataAccess dataAccess,
    string emailAddress)
  {
    if (organization.Id == Guid.Empty)
    {
      organization.Id = Guid.NewGuid();
      organization.ApiKey = Guid.NewGuid().ToString().Replace("-", "");
    }

    await dataAccess.SaveAsync(organization, emailAddress);

    return organization;
  }
}