using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Services.Sprint;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetAllSprints()
        {
            try
            {
                var result = await sprintService.GetAllSprints();

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
        [HttpPost("create-sprint")]
        public async Task<IActionResult> CreateSprint(Request.CreateSprint request)
        {
            try
            {
                var result = await sprintService.CreateNewSprintAsync(request);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Unexpected error occurred.");
            }
        }


    }
}