using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.UserTask;

public class UserTaskInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BasicInfoDto Group { get; set; } = new BasicInfoDto { };
    public List<TaskInfoDto> TodoTasks { get; set; } = new List<TaskInfoDto> { };
    public List<TaskInfoDto> DoingTasks { get; set; } = new List<TaskInfoDto> { };
    public int BacklogCount { get; set; }
}
