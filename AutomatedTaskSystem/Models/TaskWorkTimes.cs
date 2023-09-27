using AutomatedTaskSystem.Models.Enums.TaskDurationEndReason;

namespace AutomatedTaskSystem.Models;

public class TaskWorkTime
{
    public int Id { get; set; }
    public Task Task { get; set; } = new Task { };
    public int TaskId { get; set; }
    public User User { get; set; } = new User { };
    public int UserId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime? EndDate { get; set; } = null;
	public double Duration { get; set; }
	public TaskDurationEndReasonEnum? EndReason { get; set; } = null;
}
