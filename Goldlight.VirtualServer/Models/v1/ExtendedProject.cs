using System.Runtime.Serialization;
using System.Text.Json;
using Goldlight.Database.Models.v1;

namespace Goldlight.VirtualServer.Models.v1;

public class ExtendedProject : Project
{
  public ExtendedProject()
  {
  }

  public ExtendedProject(Project project)
  {
    Organization = project.Organization;
    Name = project.Name;
    Description = project.Description;
    FriendlyName = project.FriendlyName;
  }

  [DataMember]
  public Guid Id { get; set; }

  public virtual ProjectTable ToTable(int modelVersion = 1) =>
    new()
    {
      Id = Id.ToString(),
      OrganizationId = Organization,
      Details = new ()
      {
        Description = Description,
        FriendlyName = FriendlyName,
        Name = Name
      },
      ModelVersion = modelVersion,
      Version = Version
    };

  public static ExtendedProject FromTable(ProjectTable table)
  {
    ExtendedProject project = new ExtendedProject
    {
      Id = Guid.Parse(table.Id),
      Organization = table.OrganizationId,
      Name = table.Details.Name,
      Description = table.Details.Description,
      FriendlyName = table.Details.FriendlyName,
      Version = table.Version
    };
    return project;
  }
}