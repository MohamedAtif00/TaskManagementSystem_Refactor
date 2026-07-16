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
        [FromQuery(Name = "problemType")] string? problemType,
        [FromQuery] string? priority,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5) =>
        await _dailyReportService.GetDashboardAsync(
            BuildFilter(from, to, team, semester, subject, grade, taskName, status, problemType, priority, page, pageSize));

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
        [FromQuery(Name = "problemType")] string? problemType,
        [FromQuery] string? priority,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5) =>
        await _dailyReportService.GetRowsAsync(BuildFilter(from, to, team, semester, subject, grade, taskName, status, problemType, priority, page, pageSize));

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
        [FromQuery(Name = "problemType")] string? problemType,
        [FromQuery] string? priority) =>
        await _dailyReportService.GetSummaryAsync(BuildFilter(from, to, team, semester, subject, grade, taskName, status, problemType, priority));

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
        [FromQuery(Name = "problemType")] string? problemType,
        [FromQuery] string? priority) =>
        await _dailyReportService.GetChartsAsync(BuildFilter(from, to, team, semester, subject, grade, taskName, status, problemType, priority));

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
        string? problemType,
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
            ProblemType = problemType,
            Priority = priority,
            Page = page,
            PageSize = pageSize
        };
}
