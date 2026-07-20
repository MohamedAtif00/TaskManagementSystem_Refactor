namespace AutomatedTaskSystem.Dtos.TaskLogger;

public class GetTaskLoggerSummaryDto
{
    public int TotalTasks { get; set; }
    public double TotalTimeMinutes { get; set; }
    public int RollbackTasks { get; set; }
    public double TotalPoints { get; set; }
}
