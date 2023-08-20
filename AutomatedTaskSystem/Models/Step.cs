namespace AutomatedTaskSystem.Models;

public class Step
{
    public bool Archived { get; set; } = false;
    public int Id { get; set; }
    public int Order { get; set; }
    public Node Node { get; set; } = new Node { };
    public int NodeId { get; set; }
    public TaskBank TaskBank { get; set; } = new TaskBank { };
    public int TaskBankId { get; set; }
    public int Duration { get; set; }
    public int? Priority { get; set; } = null;
    public List<Models.Task> Tasks { get; set; } = new List<Task> { };
    public List<Step> Rollbacks { get; set; } = new List<Step> { };
    public List<Step> From { get; set; } = new List<Step> { };
}
