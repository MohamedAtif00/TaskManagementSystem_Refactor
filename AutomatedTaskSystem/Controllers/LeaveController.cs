using System.Threading.Tasks;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Services.Leave;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers
{
    [Route("[controller]")]
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
        public async Task<IActionResult> GetAllLeaves([FromQuery] int? userId,int? role)
        {
            var result = await _leaveRequestService.GetAllVacationsAsync(userId,role);
            // Logic to get all leaves
            return Ok(result);
        }
        // GET: api/Leave/{id}
        [HttpGet("{id}")]
        public IActionResult GetLeaveById(int id)
        {

            // Logic to get leave by id
            return Ok(new { message = $"Leave details for ID: {id}" });
        }
        //[HttpGet("GetBelongToTM/{id}")]
        //public async Task<IActionResult> GetBelongToTm()
        //{
            
        //}


        [HttpGet("LeaveRequestsByUserId/{id}")]
        public async Task<IActionResult> GetLeavesByUserId(int id) 
        {
            var leaves = await _leaveRequestService.GetLeaveRequestsByUserId(id);

            return Ok(leaves);
        }

        [HttpGet("LeaveRequestByUserId/{id}")]
        public async Task<IActionResult> GetLeaveByUserId(int id)
        {
            var leaves = await _leaveRequestService.GetLeaveRequestByUserId(id);

            return Ok(leaves);
        }
        [HttpGet("GetSingleOpinion")]
        public async Task<IActionResult> GetSingleOpinion(int id)
        {
            var opinion = await _leaveRequestService.GetSingleOpinion(id);
            return Ok(opinion);
        }
        [HttpPost("CreateOpinion")]
        public async Task<IActionResult> CreateOpinion([FromBody] CreateOpinionDto opinion)
        {
            // Validate the opinion object
            if (opinion == null)
            {
                return BadRequest("Opinion data is required.");
            }
            var result = await _leaveRequestService.GiveOpinion(opinion);
            // Logic to create a new opinion
            return CreatedAtAction(nameof(GetLeaveById), new { id = 1 }, opinion);
        }
        // POST: api/Leave
        [HttpPost]
        public async Task<IActionResult> CreateLeave([FromBody] CreateLeaveRequestDto leave)
        {

            // Validate the leave object
            if (leave == null)
            {
                return BadRequest("Leave data is required.");
            }

            var result = await _leaveRequestService.CreateLeaveRequest(leave);
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
