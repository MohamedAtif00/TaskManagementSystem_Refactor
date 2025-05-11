using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Services.Leave;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;

        public LeaveController(ILeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }

        // GET: api/Leave
        [HttpGet]
        public IActionResult GetAllLeaves()
        {
            // Logic to get all leaves
            return Ok(new { message = "List of all leaves" });
        }
        // GET: api/Leave/{id}
        [HttpGet("{id}")]
        public IActionResult GetLeaveById(int id)
        {
            // Logic to get leave by id
            return Ok(new { message = $"Leave details for ID: {id}" });
        }
        // POST: api/Leave
        [HttpPost]
        public IActionResult CreateLeave([FromBody] CreateLeaveRequestDto leave)
        {

            // Validate the leave object
            if (leave == null)
            {
                return BadRequest("Leave data is required.");
            }

            _leaveRequestService.CreateLeaveRequest(leave);
            // Logic to create a new leave
            return CreatedAtAction(nameof(GetLeaveById), new { id = 1 }, leave);
        }
        // PUT: api/Leave/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateLeave(int id, [FromBody] object leave)
        {
            // Logic to update leave by id
            return NoContent();
        }
        // DELETE: api/Leave/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteLeave(int id)
        {
            // Logic to delete leave by id
            return NoContent();
        }
    }
}
