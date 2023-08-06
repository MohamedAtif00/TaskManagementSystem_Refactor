using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class GetCreatableTasks
{
    public List<BasicInfoDto> LearningObjectives { get; set; } = new List<BasicInfoDto> { };
    public List<BasicInfoDto> Assignees { get; set; } = new List<BasicInfoDto> { };
    public List<TaskOption> Options { get; set; } = new List<TaskOption> { };
}
