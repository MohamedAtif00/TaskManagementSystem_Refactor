namespace AutomatedTaskSystem.Dtos.DailyReport;

public class GetDailyReportChartDto
{
    public int Approved { get; set; }
    public int Hold { get; set; }
    public int Rollback { get; set; }
    public List<DailyReportProblemCountDto> ProblemTypes { get; set; } = new();
}

public class DailyReportProblemCountDto
{
    public string ProblemType { get; set; } = "";
    public int Count { get; set; }
}
