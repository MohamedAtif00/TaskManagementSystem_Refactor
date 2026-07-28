using AutomatedTaskSystem.Dtos.TaskLogger;
using AutomatedTaskSystem.Services.TaskLogger;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers;

[Route("task-logger")]
[ApiController]
public class TaskLoggerController : ControllerBase
{
    private readonly ITaskLoggerService _taskLoggerService;

    public TaskLoggerController(ITaskLoggerService taskLoggerService)
    {
        _taskLoggerService = taskLoggerService;
    }

    [Authorize]
    [HttpGet("dashboard")]
    public async Task<ActionResult<ResponseService<GetTaskLoggerDashboardDto>>> GetDashboard(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? search,
        [FromQuery] string? member,
        [FromQuery] string? subject,
        [FromQuery] string? status,
        [FromQuery(Name = "taskName")] string? taskName,
        [FromQuery(Name = "rankingRange")] string? rankingRange,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5) =>
        await _taskLoggerService.GetDashboardAsync(BuildFilter(
            from, to, search, member, subject, status, taskName, rankingRange, page, pageSize));

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<ResponseService<GetTaskLoggerPagedDto>>> GetRows(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? search,
        [FromQuery] string? member,
        [FromQuery] string? subject,
        [FromQuery] string? status,
        [FromQuery(Name = "taskName")] string? taskName,
        [FromQuery(Name = "rankingRange")] string? rankingRange,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5) =>
        await _taskLoggerService.GetRowsAsync(BuildFilter(
            from, to, search, member, subject, status, taskName, rankingRange, page, pageSize));

    [Authorize]
    [HttpGet("lookups")]
    public async Task<ActionResult<ResponseService<GetTaskLoggerLookupsDto>>> GetLookups() =>
        await _taskLoggerService.GetLookupsAsync();

    private static TaskLoggerFilterDto BuildFilter(
        DateTime? from,
        DateTime? to,
        string? search,
        string? member,
        string? subject,
        string? status,
        string? taskName,
        string? rankingRange,
        int page = 1,
        int pageSize = 5) =>
        new()
        {
            From = from,
            To = to,
            Search = search,
            Member = member,
            Subject = subject,
            Status = status,
            TaskName = taskName,
            RankingRange = rankingRange ?? "all",
            Page = page,
            PageSize = pageSize
        };
}
