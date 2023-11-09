using Asp.Versioning;
using Asp.Versioning.Builder;
using Goldlight.Database;
using Goldlight.Database.DatabaseOperations;
using Goldlight.Database.Models.v1;
using Goldlight.VirtualServer.Extensions;
using Goldlight.VirtualServer.Models;
using Goldlight.VirtualServer.Models.v1;
using Goldlight.VirtualServer.VirtualRequest;
using Keycloak.AuthServices.Authentication;
using LocalStack.Client.Extensions;

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

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllAllowed");

app.UseMiddleware<VirtualRequestHandler>();

app.RegisterOrganizationEndpoints(version1);
app.RegisterProjectEndpoints(version1);
app.RegisterUserEndpoints(version1);

app.Run();