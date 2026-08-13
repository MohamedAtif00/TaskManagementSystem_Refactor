using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Dtos.SprintDtos;
using AutomatedTaskSystem.Services.ResponseService;
using AutomatedTaskSystem.Services.Sprint;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static AutomatedTaskSystem.DTO.Request;

namespace AutomatedTaskSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SprintController : ControllerBase
    {
        private ISprintService sprintService;
        public SprintController(ISprintService sprintService)
        {
            this.sprintService = sprintService;
        }


        [HttpGet("get-all-sprint")]
        public async Task<IActionResult> GetAllSprints(
            [FromQuery] bool? archived = null,
            [FromQuery] string? yearName = null,
            [FromQuery] string? projectName = null,
            [FromQuery] string? termName = null,
            [FromQuery] string? subjectGroupName = null)
        {
            try
            {
                var hierarchyFilter = new Dtos.SprintDtos.SprintHierarchyFilter
                {
                    YearName = yearName,
                    ProjectName = projectName,
                    TermName = termName,
                    SubjectGroupName = subjectGroupName,
                };

                var result = await sprintService.GetAllSprints(archived, hierarchyFilter);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSingleSprint(int id)
        {

            try
            {
                var result = await sprintService.GetSingleSprintAsync(id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        [HttpPost("resolve-los-by-name")]
        public async Task<IActionResult> ResolveLosByName([FromBody] ResolveLosByName request)
        {
            try
            {
                var result = await sprintService.ResolveLosByNameAsync(request);
                if (result.Error)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Unexpected error occurred.");
            }
        }

        [HttpPost("create-sprint")]
        public async Task<IActionResult> CreateSprint(Request.CreateSprint request)
        {
            try
            {
                var result = await sprintService.CreateNewSprintAsync(request);
                if (result.Error)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Unexpected error occurred.");
            }
        }

        /// <summary>
        /// Updates an existing sprint.
        /// </summary>
        /// <param name="id">The ID of the sprint to update (from route).</param>
        /// <param name="request">The request body containing updated sprint details.</param>
        /// <returns>The updated SprintDTO if successful, otherwise an error response.</returns>
        [HttpPut("update-sprint/{id}")] // Route for updating a sprint
        public async Task<IActionResult> UpdateSprint(int id, [FromBody] UpdateSprint request) // [FromBody] ensures the request body is correctly deserialized
        {
            try
            {
                // Call the service method to update the sprint
                var result = await sprintService.UpdateSprintAsync(id, request);

                // Check the service's response for errors
                if (result.Error)
                {
                    // If the service reports an error (e.g., sprint not found, validation error),
                    // return a BadRequest (400) or NotFound (404) with the error message.
                    if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    {
                        return NotFound(result);
                    }
                    return BadRequest(result);
                }

                // If successful, return OK (200) with the updated sprint data
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes (use a proper logging framework in production)
                Console.WriteLine($"Error in UpdateSprint controller: {ex.Message}");
                Console.WriteLine(ex.StackTrace);

                // Return a 500 Internal Server Error for unhandled exceptions
                return StatusCode(500, new BaseResponseService { Error = true, Message = $"An unexpected server error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Archives or unarchives a sprint.
        /// </summary>
        /// <param name="id">The ID of the sprint to archive/unarchive.</param>
        /// <param name="archived">True to archive, false to unarchive.</param>
        /// <returns>The updated sprint if successful, otherwise an error response.</returns>
        [HttpPatch("{id}/archive")]
        public async Task<IActionResult> ArchiveSprint(int id, [FromQuery] bool archived = true)
        {
            try
            {
                var result = await sprintService.ArchiveSprintAsync(id, archived);

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
                Console.WriteLine($"Error in ArchiveSprint controller: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, new BaseResponseService { Error = true, Message = $"An unexpected server error occurred: {ex.Message}" });
            }
        }



    }
}