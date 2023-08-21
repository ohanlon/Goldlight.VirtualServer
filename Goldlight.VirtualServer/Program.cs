using System.Diagnostics;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Goldlight.Database;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Database.Models.v1;
using Goldlight.VirtualServer.Models.v1;
using LocalStack.Client.Extensions;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

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
  options.ApiVersionReader = new MediaTypeApiVersionReader();
});

var app = builder.Build();

ApiVersionSet organizations = app.NewApiVersionSet("Organizations").Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/organization", async (OrganizationDataAccess oda, [FromBody] Organization organization) =>
{
  OrganizationTable organizationTable = new()
  {
    Id = organization.Id.ToString(),
    Name = organization.Name,
    ModelVersion = 1
  };
  await oda.SaveOrganizationAsync(organizationTable);

  return TypedResults.Created($"/api/organizations/{organization.Id}", organization);
}).WithApiVersionSet(organizations).HasApiVersion(version1);

app.MapGet("/api/organizations", async (OrganizationDataAccess oda) =>
{
  IEnumerable<OrganizationTable> allOrganizations = await oda.GetOrganizationsAsync();
  return allOrganizations
    .Select(organization => new Organization { Id = Guid.Parse(organization.Id!), Name = organization.Name });
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

