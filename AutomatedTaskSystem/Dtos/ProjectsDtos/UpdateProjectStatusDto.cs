using AutomatedTaskSystem.Models.Enums.ProjectStatus;

namespace AutomatedTaskSystem.Dtos.Projects;

public class UpdateProjectStatusDto
{
    public ProjectStatusEnum Status { get; set; } = ProjectStatusEnum.Active;
}
