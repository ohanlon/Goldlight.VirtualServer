using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Goldlight.Database;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Database.Models.v1;
using Goldlight.VirtualServer.Models;
using Goldlight.VirtualServer.Models.v1;
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

var app = builder.Build();

ApiVersionSet organizations = app.NewApiVersionSet("Organizations").Build();
ApiVersionSet projects = app.NewApiVersionSet("Projects").Build();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllAllowed");

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

app.MapPost("/api/organization", async (OrganizationDataAccess dataAccess, Organization organization) =>
{
  ExtendedOrganization extendedOrganization = new(organization)
  {
    ApiKey = Guid.NewGuid().ToString().Replace("-", "")
  };
  await dataAccess.SaveOrganizationAsync(extendedOrganization.ToTable());
  extendedOrganization.Version = 0;
  return TypedResults.Created($"/api/organization/{organization.Id}", extendedOrganization);
}).WithApiVersionSet(organizations).HasApiVersion(version1);

app.MapGet("/api/organization/{id}", async Task<Results<Ok<ExtendedOrganization>, NotFound>> (OrganizationDataAccess oda, string id) =>
{
  OrganizationTable? organization = await oda.GetOrganizationAsync(id);
  if (organization is null)
  {
    return TypedResults.NotFound();
  }
  return TypedResults.Ok(ExtendedOrganization.FromTable(organization));
}).WithApiVersionSet(organizations).HasApiVersion(version1);

app.MapGet("/api/organizations", async (OrganizationDataAccess dataAccess) =>
{
  IEnumerable<OrganizationTable> allOrganizations = await dataAccess.GetOrganizationsAsync();
  return allOrganizations
    .Select(ExtendedOrganization.FromTable);
}).WithApiVersionSet(organizations).HasApiVersion(version1);

app.MapPut("/api/organization", async (OrganizationDataAccess dataAccess, Organization organization) =>
{
  await dataAccess.SaveOrganizationAsync(organization.ToTable());
  organization.Version++;
  return TypedResults.Ok(organization);
}).WithApiVersionSet(organizations).HasApiVersion(version1);

app.MapDelete("/api/organization/{id}", async (OrganizationDataAccess dataAccess, string id) =>
{
  await dataAccess.DeleteOrganizationAsync(id);
  return TypedResults.Ok();
}).WithApiVersionSet(organizations).HasApiVersion(version1);

app.Use(async (context, next) =>
{
  if (context.Request.Path.Value!.StartsWith("/api/"))
  {
    await next.Invoke();
    return;
  }
  await WriteResponse(context, "Hello ServiceVirtualization");
});

app.Run();

static async Task WriteResponse<T>(HttpContext context, T result, int statusCode = 200, string contentType = "application/json")
{
  HttpResponse response = context.Response;
  response.StatusCode = statusCode;
  response.ContentType = contentType;
  await response.WriteAsync(JsonSerializer.Serialize(result));
}

