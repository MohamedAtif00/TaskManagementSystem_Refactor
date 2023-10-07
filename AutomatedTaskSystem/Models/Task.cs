using AutomatedTaskSystem.Models.Enums.TaskPriority;
using AutomatedTaskSystem.Models.Enums.TaskStatus;
using System.ComponentModel.DataAnnotations;

namespace AutomatedTaskSystem.Models;

public class Task
{
    public bool Archived { get; set; } = false;
    public bool Pause { get; set; } = false;
    public bool Attention { get; set; } = false;
    public int Id { get; set; }
    public string Name { get; set; } = "";
	[Range(0, 4)]
	public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Backlog;
    public Step? Step { get; set; }
    public int? StepId { get; set; }
    public bool TL { get; set; } = false;
    public Group Group { get; set; } = new Group { };
    public int GroupId { get; set; }
    public Task? From { get; set; }
    public int? FromId { get; set; }
    public User? User { get; set; }
    public int? UserId { get; set; }
    public LearningObjective LearningObjective { get; set; } = new LearningObjective { };
    public int LearningObjectiveId { get; set; }
    public bool IsReview { get; set; } = false;
    public bool Flagged { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int RollbackCount { get; set; }
    public bool IsRollback { get; set; } = false;
	[Range(0, 3)]
    public TaskPriorityEnum Priority { get; set; } = TaskPriorityEnum.None;
	public List<Comment> Comments { get; set; } = new List<Comment>{};
}
