using AutomatedTaskSystem.Dtos.DailyReport;

namespace AutomatedTaskSystem.Dtos.DailyReport;

public class DailyReportFilterDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Team { get; set; }
    public string? Semester { get; set; }
    public string? Subject { get; set; }
    public string? Grade { get; set; }
    public string? TaskName { get; set; }
    public string? Status { get; set; }
    public string? ProblemType { get; set; }
    public string? Priority { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}
