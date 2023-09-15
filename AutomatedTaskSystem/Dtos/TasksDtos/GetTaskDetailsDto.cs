using AutomatedTaskSystem.Dtos.Common;
using AutomatedTaskSystem.Models.Enums.TaskPriority;
using AutomatedTaskSystem.Models.Enums.TaskStatus;

namespace AutomatedTaskSystem.Dtos.Tasks;

public class GetTaskDetailsDto
{
    public bool Pause { get; set; }
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public BasicInfoDto LearningObjective { get; set; } = new BasicInfoDto { };
    public string Tag { get; set; } = "";
    public string Template { get; set; } = "";
    public string Environment { get; set; } = "";
    public BasicInfoDto Schema { get; set; } = new BasicInfoDto { };
    public BasicInfoDto? User { get; set; } = null;
    public bool IsReview { get; set; }
    public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Backlog;
    public bool Flagged { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? DoneAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public TaskPriorityEnum Priority { get; set; } = TaskPriorityEnum.None;
    public List<TaskCommentDto> Comments { get; set; } = new List<TaskCommentDto> { };
    public TaskAccess Access { get; set; } = TaskAccess.WorkOn;
}
