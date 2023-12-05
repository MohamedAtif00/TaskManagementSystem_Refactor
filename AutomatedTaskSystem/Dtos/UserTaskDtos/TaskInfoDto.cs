using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.UserTask;

public class TaskInfoDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BasicInfoDto LearningObjective { get; set; } = new BasicInfoDto { };
    public int ProjectId { get; set; }
}
