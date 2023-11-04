using AutomatedTaskSystem.Dtos.Common;

namespace AutomatedTaskSystem.Dtos.UserTask;

public class UserTaskDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BasicInfoDto Group { get; set; } = new BasicInfoDto { };
    public TaskCountDto Tasks { get; set; } = new TaskCountDto { };
}
