using System.Security.Claims;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Services.Leave;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VacationController : ControllerBase
    {
        private readonly LeaveRequestService _vacationService;

        public VacationController(LeaveRequestService vacationService)
        {
            _vacationService = vacationService;
        }

        // 🔹 CREATE
        [HttpPost]
        public async Task<IActionResult> CreateVacation([FromBody] CreateLeaveRequestDto request)
        {
            var result = await _vacationService.CreateLeaveRequest(request);
            if (result.Error)
                return BadRequest(result.Message);
            return Ok("Vacation created successfully.");
        }

        // 🔹 READ ALL
        [HttpGet]
        public async Task<IActionResult> GetAllVacations([FromQuery] int? userId = null)
        {
            var result = await _vacationService.GetAllVacationsAsync(userId);
            return Ok(result);
        }

        // 🔹 READ BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVacationById(int id)
        {
            var result = await _vacationService.GetVacationByIdAsync(id);
            if (result.Value.Data == null)
                return NotFound(result.Value);
            return Ok(result.Value);
        }

        // 🔹 UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVacation(int id, [FromBody] UpdateLeaveRequestDto request)
        {
            var result = await _vacationService.UpdateVacationAsync(
                id,
                DateTime.Parse(request.StartDate),
                DateTime.Parse(request.EndDate),
                request.Reason,
                request.Status);

            if (!result)
                return NotFound("Vacation not found.");
            return Ok("Vacation updated successfully.");
        }

        // 🔹 DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVacation(int id)
        {
            var result = await _vacationService.DeleteVacationAsync(id);
            if (!result)
                return NotFound("Vacation not found.");
            return Ok("Vacation deleted successfully.");
        }
    }
}
