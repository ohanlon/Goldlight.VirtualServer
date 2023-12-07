using Dapper;
using Goldlight.Models;
using System.Data;
using Goldlight.ExceptionManagement;
using Goldlight.Models.RequestResponse;

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

  public virtual async Task DeletePairsAsync(Guid id) =>
    _ = await ExecuteCommandAsync("DELETE FROM sv.\"RequestResponse\" WHERE id = @id", new { id });

  public virtual async Task<List<RequestResponsePair>> GetAllRequestResponsesForProjectAsync(Guid projectId)
  {
    List<RequestResponsePair> pairs = new();
    string sql =
      "SELECT * FROM sv.request_response WHERE projectId = @projectId";
    IEnumerable<Tuple<RequestResponsePair, Request, Response, HttpRequestSummary, HttpResponseSummary>> results =
      await Connection
        .QueryAsync<RequestResponsePair, Request, Response, HttpRequestSummary, HttpResponseSummary,
          Tuple<RequestResponsePair, Request, Response, HttpRequestSummary, HttpResponseSummary>>(sql,
          (rr, rq, rs, rqs, rps) => Tuple.Create(rr, rq, rs, rqs, rps),
          new { projectId },
          splitOn: "id,requestid,responseid,requestsummaryid,responsesummaryid");
    List<Tuple<RequestResponsePair, Request, Response, HttpRequestSummary, HttpResponseSummary>> resultsTuple =
      results.ToList();
    if (!resultsTuple.Any()) return pairs;
    foreach ((RequestResponsePair rrpair, Request request, Response response, HttpRequestSummary requestSummary,
               HttpResponseSummary responseSummary) in resultsTuple)
    {
      rrpair.Request = request;
      rrpair.Response = response;
      rrpair.Request.Summary = requestSummary;
      rrpair.Response.Summary = responseSummary;
      await GetHeaders(rrpair);
      pairs.Add(rrpair);
    }

    return pairs;
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

  public virtual async Task SaveRequestResponsePairAsync(RequestResponsePair pair)
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
    _ = await ExecuteStoredProcedureAsync("sv.\"glsp_SaveRRPair\"",
      dynamicParameters, () =>
      {
        if (dynamicParameters.Get<int>("affected_rows") == 0)
        {
          throw new SaveConflictException();
        }
      });

    await SaveHeaders(pair.Request.Headers, "RequestHeader", "request", pair.Request.Id);
    await SaveHeaders(pair.Response.Headers, "ResponseHeader", "response", pair.Response.Id);
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

  private async Task GetHeaders(RequestResponsePair pair)
  {
    string requestHeader = "SELECT * FROM sv.\"RequestHeader\" WHERE request_id = @requestId";
    string responseHeader = "SELECT * FROM sv.\"ResponseHeader\" WHERE response_id = @responseId";

    var request = await Connection.QueryAsync<HttpHeader>(requestHeader, new { requestId = pair.Request.Id });
    var response =
      await Connection.QueryAsync<HttpHeader>(responseHeader, new { responseId = pair.Response.Id });
    pair.Request.Headers = request as HttpHeader[] ?? request.ToArray();
    pair.Response.Headers = response as HttpHeader[] ?? response.ToArray();
  }

  private async Task SaveHeaders(HttpHeader[]? headers, string table, string reference, Guid foreignKey)
  {
    _ = await ExecuteCommandAsync($"DELETE FROM sv.\"{table}\" WHERE {reference}_id = @foreignKey",
      new { foreignKey });
    if (headers is null || headers.Length == 0)
    {
      return;
    }

    foreach (HttpHeader header in headers)
    {
      await InsertHeader(header, table, reference, foreignKey);
    }
  }

  private async Task InsertHeader(HttpHeader header, string table, string reference, Guid foreignKey)
  {
    _ = await ExecuteCommandAsync(
      $"INSERT INTO sv.\"{table}\"(id, key, value, {reference}_id) VALUES (@id, @name, @value, @foreignKey)",
      new
      {
        id = header.Id,
        name = header.Key,
        value = header.Value,
        foreignKey
      });
  }
}