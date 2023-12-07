using Asp.Versioning;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Models;
using Goldlight.Models.RequestResponse;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Goldlight.VirtualServer.Extensions;

internal static class ProjectExtensions
{
  public static void RegisterProjectEndpoints(this WebApplication app, ApiVersion version)
  {
    RouteGroupBuilder mapGroup = app.MapGroup("Projects", version, "Projects operations");

    mapGroup.MapPost("/project", CreateOrUpdateProject);
    mapGroup.MapPost("/{organization:guid}/project/{projectId:guid}/rrpair", SaveRequestResponse);

    mapGroup.MapPut("/project", CreateOrUpdateProject);
    mapGroup.MapPut("/{organization:guid}/project/{projectId:guid}/rrpair", SaveRequestResponse);

    mapGroup.MapGet("/{organization:guid}/projects/", GetProjects);
    mapGroup.MapGet("/{organization:guid}/project/{id:guid}/pairs", GetPairsForProject);

    mapGroup.MapDelete("{organization:guid}/project/{id:guid}", DeleteProject);
    mapGroup.MapDelete("{organization:guid}/project/{id:guid}/rrpair/{pairid:guid}", DeletePairs);
  }

  private static async Task<Ok<Guid>> DeleteProject(ProjectDataAccess dataAccess, Guid organization, Guid id,
    UserDataAccess userDataAccess, HttpContext context)
  {
    await context.CheckUserCanEdit(userDataAccess, organization, id);
    await dataAccess.DeleteProjectAsync(id);
    return TypedResults.Ok(id);
  }

  private static async Task<Ok<Guid>> DeletePairs(ProjectDataAccess dataAccess, Guid organization, Guid id, Guid pairid,
    UserDataAccess userDataAccess, HttpContext context)
  {
    await context.CheckUserCanEdit(userDataAccess, organization, id);
    await dataAccess.DeletePairsAsync(pairid);
    return TypedResults.Ok(pairid);
  }

  private static async Task<Ok<IEnumerable<Project>>> GetProjects(ProjectDataAccess dataAccess, Guid organization,
    HttpContext context, UserDataAccess userDataAccess)
  {
    await context.CheckUserHasAccess(userDataAccess, organization);
    var result = await dataAccess.GetProjectsAsync(organization);
    return TypedResults.Ok(result);
  }

  private static async Task<Ok<List<RequestResponsePair>>> GetPairsForProject(ProjectDataAccess dataAccess,
    Guid organization,
    Guid id, HttpContext context, UserDataAccess userDataAccess)
  {
    await context.CheckUserHasAccess(userDataAccess, organization);
    var rrpairs = await dataAccess.GetAllRequestResponsesForProjectAsync(id);
    return TypedResults.Ok(rrpairs);
  }

  private static async Task<Results<Created<Project>, Ok<Project>>>
    CreateOrUpdateProject(HttpContext context, Project project, ProjectDataAccess dataAccess,
      UserDataAccess userDataAccess) =>
    await SaveProject(context, project, dataAccess, userDataAccess) is { Version: > 0 } proj
      ? TypedResults.Created($"/project/{proj.Id}", proj)
      : TypedResults.Ok(project);

  private static async Task<Project> SaveProject(HttpContext context, Project project, ProjectDataAccess dataAccess,
    UserDataAccess userDataAccess)
  {
    await context.CheckUserHasAccess(userDataAccess, project.Organization);
    if (project.Id == Guid.Empty)
    {
      project.Id = Guid.NewGuid();
    }

    await dataAccess.SaveProjectAsync(project);

    return project;
  }

  private static async Task<Results<Created<RequestResponsePair>, Ok<RequestResponsePair>>>
    SaveRequestResponse(ProjectDataAccess dataAccess, UserDataAccess userDataAccess, Guid organization,
      RequestResponsePair rrpair,
      HttpContext context)
  {
    await context.CheckUserCanEdit(userDataAccess, rrpair.ProjectId, organization);
    CheckRequestResponsePair(rrpair);
    await dataAccess.SaveRequestResponsePairAsync(rrpair);

    return rrpair.Version > 0 ? TypedResults.Ok(rrpair) : TypedResults.Created($"/project/{rrpair.ProjectId}", rrpair);
  }

  private static void CheckRequestResponsePair(RequestResponsePair rrpair)
  {
    if (rrpair.Id == Guid.Empty) rrpair.Id = Guid.NewGuid();
    if (rrpair.Request.Id == Guid.Empty) rrpair.Request.Id = Guid.NewGuid();
    if (rrpair.Response.Id == Guid.Empty) rrpair.Response.Id = Guid.NewGuid();
    if (rrpair.Request.Summary.Id == Guid.Empty) rrpair.Request.Summary.Id = Guid.NewGuid();
    if (rrpair.Response.Summary.Id == Guid.Empty) rrpair.Response.Summary.Id = Guid.NewGuid();
    CheckHttpHeaders(rrpair.Request.Headers);
    CheckHttpHeaders(rrpair.Response.Headers);
  }

  private static void CheckHttpHeaders(HttpHeader[]? headers)
  {
    if (headers is null) return;
    foreach (HttpHeader header in headers)
    {
      if (header.Id == Guid.Empty)
      {
        header.Id = Guid.NewGuid();
      }
    }
  }
}