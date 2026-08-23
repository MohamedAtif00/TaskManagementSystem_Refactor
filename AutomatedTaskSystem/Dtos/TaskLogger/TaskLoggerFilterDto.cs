namespace AutomatedTaskSystem.Dtos.TaskLogger;

public class TaskLoggerFilterDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
    public string? Member { get; set; }
    public string? Subject { get; set; }
    public string? Status { get; set; }
    public string? TaskName { get; set; }
    /// <summary>all | week | month — applies to rankings only.</summary>
    public string RankingRange { get; set; } = "all";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}
