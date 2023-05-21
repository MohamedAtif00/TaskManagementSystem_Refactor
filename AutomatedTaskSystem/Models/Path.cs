namespace AutomatedTaskSystem.Models;

public class Path
{
	public int Id { get; set; }
    public Task? Task { get; set; } = null;
    public int? TaskId { get; set; } = null;
    public Step Step { get; set; } = new Step { };
    public int StepId { get; set; }
    public Step NextStep { get; set; } = new Step { };
    public int NextStepId { get; set; }
    public LearningObjective LearningObjective { get; set; } = new LearningObjective { };
    public int LearningObjectiveId { get; set; }
}
