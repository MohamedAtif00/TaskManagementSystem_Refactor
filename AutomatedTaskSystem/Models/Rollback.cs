namespace AutomatedTaskSystem.Models;

public class Rollback
{
    public int Id { get; set; }
    public Task Task { get; set; } = new Task { };
    public int TaskId { get; set; }
    public Task ToTask { get; set; } = new Task { };
    public int ToTaskId { get; set; }
    public User User { get; set; } = new User { };
    public int UserId { get; set; }
    public string? Clarification { get; set; }
    public List<RollbackIssue> RollbackIssues { get; set; } = new List<RollbackIssue> { };
    public bool Resolved { get; set; } = false;
}
