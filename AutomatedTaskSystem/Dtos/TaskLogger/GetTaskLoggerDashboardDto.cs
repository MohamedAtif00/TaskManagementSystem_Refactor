namespace AutomatedTaskSystem.Dtos.TaskLogger;

public class GetTaskLoggerDashboardDto
{
    public GetTaskLoggerPagedDto Rows { get; set; } = new();
    public GetTaskLoggerSummaryDto Summary { get; set; } = new();
    public List<TaskLoggerMemberRankDto> Rankings { get; set; } = new();
    public GetTaskLoggerLookupsDto Lookups { get; set; } = new();
}
