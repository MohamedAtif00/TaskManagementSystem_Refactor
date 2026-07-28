using AutomatedTaskSystem.Dtos.TaskLogger;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.TaskLogger;

public interface ITaskLoggerService
{
    Task<ActionResult<ResponseService<GetTaskLoggerDashboardDto>>> GetDashboardAsync(TaskLoggerFilterDto filter);
    Task<ActionResult<ResponseService<GetTaskLoggerPagedDto>>> GetRowsAsync(TaskLoggerFilterDto filter);
    Task<ActionResult<ResponseService<GetTaskLoggerLookupsDto>>> GetLookupsAsync();
}
