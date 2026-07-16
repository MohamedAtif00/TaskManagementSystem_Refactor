namespace AutomatedTaskSystem.Dtos.DailyReport;

public class GetDailyReportDashboardDto
{
    public GetDailyReportPagedDto Rows { get; set; } = new();
    public GetDailyReportSummaryDto Summary { get; set; } = new();
    public GetDailyReportChartDto Charts { get; set; } = new();
    public GetDailyReportLookupsDto Lookups { get; set; } = new();
}
