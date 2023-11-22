using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Goldlight.Database.Exceptions;
using Goldlight.Models;
using System.Data;

namespace Goldlight.Database.DatabaseOperations;

public class ProjectDataAccess : BaseDataAccess
{
  public ProjectDataAccess(PostgresConnection postgresConnection) : base(postgresConnection)
  {
  }

  public virtual async Task SaveProjectAsync(Project project)
  {
    DynamicParameters dynamicParameters = new();
    dynamicParameters.Add("p_id", value: project.Id, dbType: DbType.Guid);
    dynamicParameters.Add("p_friendlyname", value: project.FriendlyName.ToLowerInvariant(), dbType: DbType.String);
    dynamicParameters.Add("p_name", value: project.Name, dbType: DbType.String);
    dynamicParameters.Add("p_description", value: project.Description, dbType: DbType.String);
    dynamicParameters.Add("p_organization", value: project.Organization, dbType: DbType.Guid);
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

  public virtual async Task<IEnumerable<Project>> GetProjectsAsync(Guid organizationId)
  {
    SqlMapper.SetTypeMap(typeof(Project), new CustomPropertyTypeMap(
      typeof(Project), (type, columnName) => type.GetProperties().FirstOrDefault(prop =>
        prop.GetCustomAttributes(false).OfType<ColumnAttribute>().Any(attr => attr.Name == columnName))!));
    using var connection = Connection;
    string sql =
      "SELECT id, name, friendlyname, description, organization_id, version FROM sv.\"Project\" WHERE organization_id = @organizationId";
    var result = await connection.QueryAsync<Project>(sql, new { organizationId });
    return result;
  }

  public virtual async Task DeleteProjectAsync(Guid id) =>
    _ = await ExecuteCommandAsync("DELETE FROM sv.\"Project\" WHERE id = @id", new { id });
}