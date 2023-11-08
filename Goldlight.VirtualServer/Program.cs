using System.Security.Claims;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Goldlight.Database;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Database.Exceptions;
using Goldlight.Database.Models.v1;
using Goldlight.Models;
using Goldlight.VirtualServer.Extensions;
using Goldlight.VirtualServer.Models;
using Goldlight.VirtualServer.Models.v1;
using Goldlight.VirtualServer.VirtualRequest;
using Keycloak.AuthServices.Authentication;
using LocalStack.Client.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLocalStack(builder.Configuration);
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

builder.Services.AddData();

ApiVersion version1 = new(1, 0);
builder.Services.AddApiVersioning(options =>
{
  options.ReportApiVersions = true;
  options.DefaultApiVersion = version1;
  options.ApiVersionReader = ApiVersionReader.Combine(
    new MediaTypeApiVersionReader(),
    new HeaderApiVersionReader("x-api-version"));
});

builder.Services.AddCors(options =>
{
  options.AddPolicy("AllAllowed",
    policy =>
    {
      policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

builder.Services.AddKeycloakAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

WebApplication app = builder.Build();
var migration = app.Services.GetRequiredService<DatabaseMigrationDataAccess>();
await migration.MigrateDatabaseAsync();

app.UseAuthentication().UseAuthorization();

ApiVersionSet projects = app.NewApiVersionSet("Projects").Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllAllowed");

app.UseMiddleware<VirtualRequestHandler>();

app.RegisterOrganizationEndpoints(version1);

app.MapPost("/api/project", async (ProjectDataAccess dataAccess, Project project) =>
{
  ExtendedProject extendedProject = new(project)
  {
    Id = Guid.NewGuid()
  };
  await dataAccess.SaveProjectAsync(extendedProject.ToTable());
  extendedProject.Version = 0;
  return TypedResults.Created($"/api/organization/{extendedProject.Id}", extendedProject);
}).WithApiVersionSet(projects).HasApiVersion(version1);

app.MapPut("/api/project", async (ProjectDataAccess dataAccess, ExtendedProject project) =>
{
  await dataAccess.SaveProjectAsync(project.ToTable());
  project.Version++;
  return TypedResults.Ok(project);
}).WithApiVersionSet(projects).HasApiVersion(version1);

app.MapGet("/api/{organization}/projects/", async (ProjectDataAccess dataAccess, string organization) =>
{
  IEnumerable<ProjectTable> allProjects = await dataAccess.GetProjectsAsync(organization);
  return allProjects.Select(ExtendedProject.FromTable);
}).WithApiVersionSet(projects).HasApiVersion(version1);

app.MapDelete("/api/project/{id}", async (ProjectDataAccess dataAccess, string id) =>
{
  await dataAccess.DeleteProjectAsync(id);
  return TypedResults.Ok(id);
}).WithApiVersionSet(projects).HasApiVersion(version1);


app.Run();