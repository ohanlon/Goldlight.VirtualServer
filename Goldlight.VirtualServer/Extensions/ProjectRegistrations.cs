using Asp.Versioning;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Database.Exceptions;
using Goldlight.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Goldlight.VirtualServer.Extensions;

public static class ProjectRegistrations
{
  public static void RegisterProjectEndpoints(this WebApplication app, ApiVersion version)
  {
    RouteGroupBuilder mapGroup = app.MapGroup("Projects", version, "Projects operations");

    mapGroup.MapPost("/project", CreateProject);
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

    //mapGroup.MapPut("/api/project", async (ProjectDataAccess dataAccess, Project project) =>
    //{
    //  try
    //  {
    //    await dataAccess.SaveProjectAsync(project);
    //  }
    //  catch (SaveConflictException)
    //  {
    //    throw;
    //  } 

    //  project.Version++;
    //  return TypedResults.Ok(project);
    //});


    mapGroup.MapGet("/{organization}/projects/",
      async (ProjectDataAccess dataAccess, Guid organization) => await dataAccess.GetProjectsAsync(organization));

    mapGroup.MapDelete("/project/{id}", async (ProjectDataAccess dataAccess, Guid id) =>
    {
      await dataAccess.DeleteProjectAsync(id);
      return TypedResults.Ok(id);
    });
  }

  private static async Task<Results<Created<Project>, Ok<Project>, Conflict>>
    CreateProject(Project project, ProjectDataAccess dataAccess) =>
    await SaveProject(project, dataAccess) is { } proj
      ? TypedResults.Created($"/project/{proj.Id}", proj)
      : TypedResults.Conflict();

  private static async Task<Project?> SaveProject(Project project, ProjectDataAccess dataAccess)
  {
    try
    {
      if (project.Id == Guid.Empty)
      {
        project.Id = Guid.NewGuid();
      }

      await dataAccess.SaveProjectAsync(project);
    }
    catch (SaveConflictException)
    {
      return null;
    }

    return project;
  }
}