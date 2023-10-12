namespace AutomatedTaskSystem.Models;

public class RollbackIssue
{
    public int Id { get; set; }
	public Step Step { get; set; } = new Step {};
	public int StepId { get; set; }
	public string? Note { get; set; } = null;
}
