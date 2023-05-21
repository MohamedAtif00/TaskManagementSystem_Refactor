namespace AutomatedTaskSystem.Models;

public class Task
{
    public bool Archived { get; set; } = false;
    public bool Pause { get; set; } = false;
    public bool Attention { get; set; } = false;
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public Status Status { get; set; } = new Status { };
    public int StatusId { get; set; }
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
    public List<Comment> Comments { get; set; } = new List<Comment> { };
    public int LearningObjectiveId { get; set; }
    public bool IsReview { get; set; } = false;
    public bool Flagged { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int RollbackCount { get; set; }
    public bool IsRollback { get; set; } = false;
}
