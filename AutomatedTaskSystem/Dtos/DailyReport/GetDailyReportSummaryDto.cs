namespace AutomatedTaskSystem.Dtos.DailyReport;

public class GetDailyReportSummaryDto
{
    public int Total { get; set; }
    public int Approved { get; set; }
    public int Hold { get; set; }
    public int Rollback { get; set; }
    public int ActiveTeams { get; set; }
    public List<DailyReportTeamCountDto> TopTeams { get; set; } = new();
}

public class DailyReportTeamCountDto
{
    public string Team { get; set; } = "";
    public int Count { get; set; }
}
