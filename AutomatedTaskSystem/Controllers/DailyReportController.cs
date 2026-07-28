using AutomatedTaskSystem.Dtos.DailyReport;
using AutomatedTaskSystem.Services.DailyReport;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers;

[Route("daily-reports")]
[ApiController]
public class DailyReportController : ControllerBase
{
    private readonly IDailyReportService _dailyReportService;

    public DailyReportController(IDailyReportService dailyReportService)
    {
        _dailyReportService = dailyReportService;
    }

    [Authorize]
    [HttpGet("dashboard")]
    public async Task<ActionResult<ResponseService<GetDailyReportDashboardDto>>> GetDashboard(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? team,
        [FromQuery] string? semester,
        [FromQuery] string? subject,
        [FromQuery] string? grade,
        [FromQuery(Name = "taskName")] string? taskName,
        [FromQuery] string? status,
        [FromQuery(Name = "problemType")] List<string>? problemTypes,
        [FromQuery] string? priority,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5) =>
        await _dailyReportService.GetDashboardAsync(
            BuildFilter(from, to, team, semester, subject, grade, taskName, status, problemTypes, priority, page, pageSize));

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ResponseService<GetDailyReportPagedDto>>> GetRows(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? team,
        [FromQuery] string? semester,
        [FromQuery] string? subject,
        [FromQuery] string? grade,
        [FromQuery(Name = "taskName")] string? taskName,
        [FromQuery] string? status,
        [FromQuery(Name = "problemType")] List<string>? problemTypes,
        [FromQuery] string? priority,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5) =>
        await _dailyReportService.GetRowsAsync(BuildFilter(from, to, team, semester, subject, grade, taskName, status, problemTypes, priority, page, pageSize));

    [Authorize]
    [HttpGet("summary")]
    public async Task<ActionResult<ResponseService<GetDailyReportSummaryDto>>> GetSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? team,
        [FromQuery] string? semester,
        [FromQuery] string? subject,
        [FromQuery] string? grade,
        [FromQuery(Name = "taskName")] string? taskName,
        [FromQuery] string? status,
        [FromQuery(Name = "problemType")] List<string>? problemTypes,
        [FromQuery] string? priority) =>
        await _dailyReportService.GetSummaryAsync(BuildFilter(from, to, team, semester, subject, grade, taskName, status, problemTypes, priority));

    [Authorize]
    [HttpGet("charts")]
    public async Task<ActionResult<ResponseService<GetDailyReportChartDto>>> GetCharts(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? team,
        [FromQuery] string? semester,
        [FromQuery] string? subject,
        [FromQuery] string? grade,
        [FromQuery(Name = "taskName")] string? taskName,
        [FromQuery] string? status,
        [FromQuery(Name = "problemType")] List<string>? problemTypes,
        [FromQuery] string? priority) =>
        await _dailyReportService.GetChartsAsync(BuildFilter(from, to, team, semester, subject, grade, taskName, status, problemTypes, priority));

    [Authorize]
    [HttpGet("lookups")]
    public async Task<ActionResult<ResponseService<GetDailyReportLookupsDto>>> GetLookups() =>
        await _dailyReportService.GetLookupsAsync();

    [Authorize]
    [HttpPatch("{taskId}/notes")]
    public async Task<ActionResult<ResponseService<string>>> UpdateNotes(
        int taskId,
        [FromBody] UpdateDailyReportNotesDto dto) =>
        await _dailyReportService.UpdateNotesAsync(taskId, dto);

    private static DailyReportFilterDto BuildFilter(
        DateTime? from,
        DateTime? to,
        string? team,
        string? semester,
        string? subject,
        string? grade,
        string? taskName,
        string? status,
        List<string>? problemTypes,
        string? priority,
        int page = 1,
        int pageSize = 5) =>
        new()
        {
            From = from,
            To = to,
            Team = team,
            Semester = semester,
            Subject = subject,
            Grade = grade,
            TaskName = taskName,
            Status = status,
            ProblemTypes = problemTypes?
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>(),
            Priority = priority,
            Page = page,
            PageSize = pageSize
        };
}
