using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.Sprint;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers
{
    [Route("sprints/{sprintId}/analytics")]
    [ApiController]
    public class SprintAnalyticsController : ControllerBase
    {
        private readonly ISprintAnalyticsService _sprintAnalyticsService;

        public SprintAnalyticsController(ISprintAnalyticsService sprintAnalyticsService)
        {
            _sprintAnalyticsService = sprintAnalyticsService;
        }

        /// <summary>
        /// Get sprint overview analytics including task summary, tag distribution, and learning objectives summary
        /// </summary>
        /// <param name="sprintId">The ID of the sprint</param>
        /// <param name="timePeriod">Optional time period filter: 1=Today, 2=Last Week, 3=Last Month, 4=All Time (default)</param>
        /// <returns>Sprint overview analytics data</returns>
        [HttpGet("overview")]
        public async Task<IActionResult> GetSprintOverview(int sprintId, [FromQuery] int? timePeriod = null)
        {
            try
            {
                // Convert int to TimePeriodFilter enum, defaulting to AllTime if invalid or null
                TimePeriodFilter? filter = null;
                if (timePeriod.HasValue && Enum.IsDefined(typeof(TimePeriodFilter), timePeriod.Value))
                {
                    filter = (TimePeriodFilter)timePeriod.Value;
                }

                var result = await _sprintAnalyticsService.GetSprintOverviewAsync(sprintId, filter);

                if (result.Error)
                {
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSprintOverview controller: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                return StatusCode(500, new BaseResponseService
                {
                    Error = true,
                    Message = $"An unexpected server error occurred: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get learning objectives progress data for the sprint
        /// </summary>
        /// <param name="sprintId">The ID of the sprint</param>
        /// <param name="timePeriod">Optional time period filter: 1=Today, 2=Last Week, 3=Last Month, 4=All Time (default)</param>
        /// <param name="group">Optional group (tag) ID filter. When specified, only LOs with tasks assigned to this group are returned.</param>
        /// <returns>Learning objectives progress data</returns>
        [HttpGet("learning-objectives-progress")]
        public async Task<IActionResult> GetLearningObjectivesProgress(int sprintId, [FromQuery] int? timePeriod = null, [FromQuery] int? group = null)
        {
            try
            {
                // Convert int to TimePeriodFilter enum, defaulting to AllTime if invalid or null
                TimePeriodFilter? filter = null;
                if (timePeriod.HasValue && Enum.IsDefined(typeof(TimePeriodFilter), timePeriod.Value))
                {
                    filter = (TimePeriodFilter)timePeriod.Value;
                }

                var result = await _sprintAnalyticsService.GetSprintLearningObjectivesProgressAsync(sprintId, filter,group);

                if (result.Error)
                {
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLearningObjectivesProgress controller: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                return StatusCode(500, new BaseResponseService
                {
                    Error = true,
                    Message = $"An unexpected server error occurred: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get learning objectives table data for the sprint
        /// </summary>
        /// <param name="sprintId">The ID of the sprint</param>
        /// <returns>Learning objectives table data with Id, Name, Subject, StartDate, ActiveTasks, CurrentPhases, Status, Progress</returns>
        [HttpGet("learning-objectives-table")]
        public async Task<IActionResult> GetLearningObjectivesTable(int sprintId)
        {
            try
            {
                var result = await _sprintAnalyticsService.GetSprintLearningObjectivesTableAsync(sprintId);

                if (result.Error)
                {
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLearningObjectivesTable controller: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                return StatusCode(500, new BaseResponseService
                {
                    Error = true,
                    Message = $"An unexpected server error occurred: {ex.Message}"
                });
            }
        }
    }
}

