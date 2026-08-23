namespace AutomatedTaskSystem.Dtos.TaskLogger;

public class GetTaskLoggerRowDto
{
    public int TaskId { get; set; }
    public string Date { get; set; } = "";
    public string Member { get; set; } = "";
    public string LoCode { get; set; } = "";
    public string Subject { get; set; } = "";
    public string TaskName { get; set; } = "";
    public double ActualMinutes { get; set; }
    public double ExpectedMinutes { get; set; }
    public double Points { get; set; }
    public string Status { get; set; } = "";
    public string Notes { get; set; } = "";
}
