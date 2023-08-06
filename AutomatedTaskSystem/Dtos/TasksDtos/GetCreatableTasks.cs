using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BasicInfoDto Group { get; set; } = new BasicInfoDto { };
}

public class GetCreatableTasksDto
{
    public List<BasicInfoDto> LearningObjectives { get; set; } = new List<BasicInfoDto> { };
    public List<UserDto> Assignees { get; set; } = new List<UserDto> { };
    public List<TaskOption> Options { get; set; } = new List<TaskOption> { };
}
