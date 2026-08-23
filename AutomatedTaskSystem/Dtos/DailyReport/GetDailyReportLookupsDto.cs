namespace AutomatedTaskSystem.Dtos.DailyReport;

public class GetDailyReportLookupsDto
{
    public List<string> Teams { get; set; } = new();
    public List<string> Semesters { get; set; } = new();
    public List<string> Subjects { get; set; } = new();
    public List<string> Grades { get; set; } = new();
    public List<string> TaskNames { get; set; } = new();
    public List<string> ProblemTypes { get; set; } = new();
    public List<string> Priorities { get; set; } = new();
}
