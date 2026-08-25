using AutomatedTaskSystem.Services.Project;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.Sprint;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers;

[Route("subjects/{subjectId}/analytics")]
[ApiController]
public class ProjectAnalyticsController : ControllerBase
{
    private readonly IProjectAnalyticsService _projectAnalyticsService;

    public ProjectAnalyticsController(IProjectAnalyticsService projectAnalyticsService)
    {
        _projectAnalyticsService = projectAnalyticsService;
    }

    /// <summary>
    /// Project scope counts, aggregate progress, learning activities (LO status + tag distribution), and task summary.
    /// Optional time period filters tag distribution only (same rules as sprint analytics).
    /// </summary>
    [HttpGet("overview")]
    public async Task<IActionResult> GetProjectOverview(int subjectId, [FromQuery] int? timePeriod = null)
    {
        try
        {
            TimePeriodFilter? filter = null;
            if (timePeriod.HasValue && Enum.IsDefined(typeof(TimePeriodFilter), timePeriod.Value))
                filter = (TimePeriodFilter)timePeriod.Value;

            var result = await _projectAnalyticsService.GetProjectOverviewAsync(subjectId, filter);

            if (result.Error)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetProjectOverview: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, new BaseResponseService
            {
                Error = true,
                Message = $"An unexpected server error occurred: {ex.Message}"
            });
        }
    }

    [HttpGet("learning-objectives-progress")]
    public async Task<IActionResult> GetLearningObjectivesProgress(int subjectId, [FromQuery] int? timePeriod = null, [FromQuery] int? group = null, [FromQuery] bool inProgressOnly = false)
    {
        try
        {
            TimePeriodFilter? filter = null;
            if (timePeriod.HasValue && Enum.IsDefined(typeof(TimePeriodFilter), timePeriod.Value))
                filter = (TimePeriodFilter)timePeriod.Value;

            var result = await _projectAnalyticsService.GetProjectLearningObjectivesProgressAsync(subjectId, filter, group, inProgressOnly);

            if (result.Error)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetLearningObjectivesProgress (project): {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, new BaseResponseService
            {
                Error = true,
                Message = $"An unexpected server error occurred: {ex.Message}"
            });
        }
    }

    [HttpGet("in-progress-tasks")]
    public async Task<IActionResult> GetInProgressTasks(int subjectId, [FromQuery] int? timePeriod = null, [FromQuery] int? group = null)
    {
        try
        {
            TimePeriodFilter? filter = null;
            if (timePeriod.HasValue && Enum.IsDefined(typeof(TimePeriodFilter), timePeriod.Value))
                filter = (TimePeriodFilter)timePeriod.Value;

            var result = await _projectAnalyticsService.GetProjectInProgressTasksAsync(subjectId, filter, group);

            if (result.Error)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetInProgressTasks (project): {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, new BaseResponseService
            {
                Error = true,
                Message = $"An unexpected server error occurred: {ex.Message}"
            });
        }
    }

    [HttpGet("learning-objectives-table")]
    public async Task<IActionResult> GetLearningObjectivesTable(int subjectId)
    {
        try
        {
            var result = await _projectAnalyticsService.GetProjectLearningObjectivesTableAsync(subjectId);

            if (result.Error)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetLearningObjectivesTable (project): {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, new BaseResponseService
            {
                Error = true,
                Message = $"An unexpected server error occurred: {ex.Message}"
            });
        }
    }
}
