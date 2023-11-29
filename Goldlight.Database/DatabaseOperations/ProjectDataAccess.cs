using Dapper;
using Goldlight.Models;
using System.Data;
using Goldlight.ExceptionManagement;
using Goldlight.Models.RequestResponse;
using Npgsql;
using NpgsqlTypes;
using static Dapper.SqlMapper;

namespace Goldlight.Database.DatabaseOperations;

public class ProjectDataAccess : BaseDataAccess
{
  public ProjectDataAccess(PostgresConnection postgresConnection) : base(postgresConnection)
  {
    SetTypeMap(typeof(Project));
    SetTypeMap(typeof(HttpHeader));
    SetTypeMap(typeof(HttpResponseSummary));
    SetTypeMap(typeof(HttpRequestSummary));
    SetTypeMap(typeof(HttpResponseSummary));
    SetTypeMap(typeof(Request));
    SetTypeMap(typeof(RequestResponsePair));
    SetTypeMap(typeof(Response));
  }

  public virtual async Task SaveProjectAsync(Project project)
  {
    DynamicParameters dynamicParameters = new();
    dynamicParameters.Add("p_id", value: project.Id);
    dynamicParameters.Add("p_friendlyname", value: project.FriendlyName.ToLowerInvariant());
    dynamicParameters.Add("p_name", value: project.Name);
    dynamicParameters.Add("p_description", value: project.Description);
    dynamicParameters.Add("p_organization", value: project.Organization);
    dynamicParameters.Add("p_version", value: project.Version, dbType: DbType.Int64,
      direction: ParameterDirection.InputOutput);
    dynamicParameters.Add("affected_rows", value: 0, dbType: DbType.Int64,
      direction: ParameterDirection.Output);
    _ = await ExecuteStoredProcedureAsync("sv.\"glsp_SaveProject\"",
      dynamicParameters, () =>
      {
        if (dynamicParameters.Get<int>("affected_rows") == 0)
        {
          throw new SaveConflictException();
        }
      });
  }

  public virtual async Task SaveRequestResponsePair(RequestResponsePair pair)
  {
    if (pair.Id == Guid.Empty)
    {
      pair.Id = Guid.NewGuid();
    }

    DynamicParameters dynamicParameters = new();
    dynamicParameters.Add("p_id", value: pair.Id);
    dynamicParameters.Add("p_name", value: pair.Name);
    dynamicParameters.Add("p_description", value: pair.Description);
    dynamicParameters.Add("p_project_id", value: pair.ProjectId);
    dynamicParameters.Add("p_request_id", value: pair.Request.Id);
    dynamicParameters.Add("p_request_content", value: pair.Request.Content);
    dynamicParameters.Add("p_requestsummary_id", value: pair.Request.Summary.Id);
    dynamicParameters.Add("p_request_method", value: pair.Request.Summary.Method);
    dynamicParameters.Add("p_request_path", value: pair.Request.Summary.Path);
    dynamicParameters.Add("p_request_protocol", value: pair.Request.Summary.Protocol);
    dynamicParameters.Add("p_response_id", value: pair.Response.Id);
    dynamicParameters.Add("p_response_content", value: pair.Response.Content);
    dynamicParameters.Add("p_responsesummary_id", value: pair.Response.Summary.Id);
    dynamicParameters.Add("p_response_protocol", value: pair.Response.Summary.Protocol);
    dynamicParameters.Add("p_response_status", value: pair.Response.Summary.Status, dbType: DbType.Int32);
    dynamicParameters.Add("p_version", value: pair.Version, dbType: DbType.Int64,
      direction: ParameterDirection.InputOutput);
    dynamicParameters.Add("affected_rows", value: 0, dbType: DbType.Int64,
      direction: ParameterDirection.Output);
    try
    {
      _ = await ExecuteStoredProcedureAsync("sv.\"glsp_SaveRRPair\"",
        dynamicParameters, () =>
        {
          if (dynamicParameters.Get<int>("affected_rows") == 0)
          {
            throw new SaveConflictException();
          }
        });
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }

  public virtual async Task<IEnumerable<Project>> GetProjectsAsync(Guid organizationId)
  {
    using var connection = Connection;
    string sql =
      "SELECT id, name, friendlyname, description, organization_id, version FROM sv.\"Project\" WHERE organization_id = @organizationId";
    var result = await connection.QueryAsync<Project>(sql, new { organizationId });
    return result;
  }

  public virtual async Task DeleteProjectAsync(Guid id) =>
    _ = await ExecuteCommandAsync("DELETE FROM sv.\"Project\" WHERE id = @id", new { id });
}

public class JsonParameter : ICustomQueryParameter
{
  private readonly string _value;

  public JsonParameter(string value)
  {
    _value = value;
  }

  public void AddParameter(IDbCommand command, string name)
  {
    var parameter = new NpgsqlParameter(name, NpgsqlDbType.Json);
    parameter.Value = _value;

    command.Parameters.Add(parameter);
  }
}