using AutomatedTaskSystem.Models.Enums.ProjectStatus;

namespace AutomatedTaskSystem.Dtos.Projects;

public class UpdateProjectStatusDto
{
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
}
