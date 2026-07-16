namespace AutomatedTaskSystem.Dtos.DailyReport;

public class GetDailyReportPagedDto
{
    public List<GetDailyReportRowDto> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
