using Amazon.DynamoDBv2.Model;
using Asp.Versioning;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Models;
using Goldlight.Models.RequestResponse;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Goldlight.VirtualServer.Extensions;

public static class ProjectExtensions
{
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

  public static void RegisterProjectEndpoints(this WebApplication app, ApiVersion version)
  {
    RouteGroupBuilder mapGroup = app.MapGroup("Projects", version, "Projects operations");

    mapGroup.MapPost("/project", CreateOrUpdateProject);
    mapGroup.MapPut("/project", CreateOrUpdateProject);

    mapGroup.MapPost("/{projectId}/rrpair",
      async (ProjectDataAccess dataAccess, UserDataAccess userDataAccess, RequestResponsePair rrpair,
        HttpContext context) =>
      {
        await userDataAccess.CheckUserCanEdit(rrpair.ProjectId, context.EmailAddress());
        CheckRequestResponsePair(rrpair);
        await dataAccess.SaveRequestResponsePair(rrpair);
        return TypedResults.Ok(rrpair);
      });

    mapGroup.MapGet("/{organization}/projects/",
      async (ProjectDataAccess dataAccess, Guid organization) => await dataAccess.GetProjectsAsync(organization));

    mapGroup.MapDelete("/project/{id}", async (ProjectDataAccess dataAccess, Guid id) =>
    {
      await dataAccess.DeleteProjectAsync(id);
      return TypedResults.Ok(id);
    });
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
    await userDataAccess.CheckUserCanEdit(project.Id, context.EmailAddress());
    if (project.Id == Guid.Empty)
    {
      project.Id = Guid.NewGuid();
    }

    await dataAccess.SaveProjectAsync(project);

    return project;
  }
}