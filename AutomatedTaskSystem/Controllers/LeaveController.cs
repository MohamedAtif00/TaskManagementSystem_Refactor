using System.Threading.Tasks;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Models.Configs;
using AutomatedTaskSystem.Services.Leave;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace AutomatedTaskSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveRequestService _leaveRequestService;
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly LeaveSettings _leaveSettings;

        public LeaveController(ILeaveRequestService leaveRequestService, DataContext dataContext, IWebHostEnvironment webHostEnvironment, IOptions<LeaveSettings> leaveSettings)
        {
            _leaveRequestService = leaveRequestService;
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
            _leaveSettings = leaveSettings?.Value ?? new LeaveSettings();
        }

        [HttpGet("settings")]
        public IActionResult GetLeaveSettings()
        {
            var today = DateTime.Today;
            int year = today.Year;
            var cutoff = LeaveSettings.ParseDateForYear(_leaveSettings.EmergencyBlackoutCutoffDate, year);
            var resetDate = LeaveSettings.ParseDateForYear(_leaveSettings.ResetDate, year);
            bool emergencyAllowed = (cutoff == null && resetDate == null) ||
                (cutoff != null && today <= cutoff.Value) ||
                (resetDate != null && today >= resetDate.Value);

            var windowStart = LeaveSettings.ParseDateForYear(_leaveSettings.FromNextBalanceStartDate, year);
            var windowEnd = LeaveSettings.ParseDateForYear(_leaveSettings.FromNextBalanceEndDate, year);
            bool inFromNextWindow = windowStart != null && windowEnd != null && today >= windowStart.Value && today <= windowEnd.Value;
            // After reset date, FromNextBalance must not be available (even if within window or 0 annual leave)
            bool fromNextWindowActive = inFromNextWindow && (resetDate == null || today < resetDate.Value);

            var dto = new LeaveSettingsDto
            {
                FromNextBalanceMaxDays = _leaveSettings.FromNextBalanceMaxDays,
                FromNextBalanceStartDate = _leaveSettings.FromNextBalanceStartDate,
                FromNextBalanceEndDate = _leaveSettings.FromNextBalanceEndDate,
                EmergencyBlackoutCutoffDate = _leaveSettings.EmergencyBlackoutCutoffDate,
                ResetDate = _leaveSettings.ResetDate,
                EmergencyAllowed = emergencyAllowed,
                FromNextBalanceWindowActive = fromNextWindowActive
            };
            return Ok(dto);
        }

        [HttpPost("preview")]
        public async Task<IActionResult> PreviewAnnualLeave([FromBody] LeavePreviewRequestDto? request)
        {
            if (request == null)
            {
                return BadRequest(new ResponseService<LeavePreviewDto>
                {
                    Error = true,
                    Message = "Request body is required. Send JSON: { userId, startDate, endDate }.",
                    Data = new LeavePreviewDto { ErrorMessage = "Request body is required." }
                });
            }
            if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.StartDate) || string.IsNullOrWhiteSpace(request.EndDate))
            {
                return BadRequest(new ResponseService<LeavePreviewDto>
                {
                    Error = true,
                    Message = "UserId, StartDate and EndDate are required.",
                    Data = new LeavePreviewDto { ErrorMessage = "UserId, StartDate and EndDate are required." }
                });
            }
            var result = await _leaveRequestService.PreviewAnnualLeave(request.UserId, request.StartDate.Trim(), request.EndDate.Trim());
            if (result.Error)
                return BadRequest(result);
            return Ok(result);
        }

        // GET: api/Leave
        [HttpGet]
        public async Task<IActionResult> GetAllLeaves(
            [FromQuery] int? userId,
            [FromQuery] int? role,
            [FromQuery] int page = 1, // Add pagination parameter
            [FromQuery] int pageSize = 10, // Add pagination parameter
            [FromQuery] string? searchTerm = null, // Add search term parameter
            [FromQuery] string? fromDate = null, // Add filter for fromDate
            [FromQuery] string? toDate = null, // Add filter for toDate
            [FromQuery] string? status = null, // Add filter for status
            [FromQuery] string? myStatus = null, // Add filter for status
            [FromQuery] string? type = null, // Add filter for type
            [FromQuery] bool disablePagination = false // Add filter for type
        )
        {
            // Pass all the received query parameters to the service method
            var result = await _leaveRequestService.GetAllVacationsAsync(
                page,
                pageSize,
                searchTerm,
                fromDate,
                toDate,
                status,
                myStatus,
                type,
                disablePagination
            );

            // Check if the service method returned an error
            if (result.Error)
            {
                return BadRequest(result); // Return 400 Bad Request if there's an error
            }

            // Return 200 OK with the paginated data
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


        [HttpGet("LeaveRequestsByUserId")] // Removed {id} from the route
        public async Task<IActionResult> GetLeaveRequestsByUserId(
            [FromQuery] int? userId,
            [FromQuery] int? role, // Keep role if you plan to use it for authorization/filtering later
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? fromDate = null,
            [FromQuery] string? toDate = null,
            [FromQuery] string? status = null,
            [FromQuery] string? type = null,
            [FromQuery] bool disablePagination = false)
        {
            // Basic validation for userId if it's mandatory
            if (!userId.HasValue)
            {
                return BadRequest(new ResponseService<PageList<GetLeaveRequestDto>>
                {
                    Error = true,
                    Message = "User ID is required."
                });
            }

            var result = await _leaveRequestService.GetLeaveRequestsByUserId(
                userId.Value, // Pass the userId as a non-nullable int
                page,
                pageSize,
                searchTerm,
                fromDate,
                toDate,
                status,
                type,
                disablePagination
            );

            if (result.Error)
            {
                // Depending on the nature of the error, you might return different status codes.
                // For general service errors, 500 is often appropriate.
                return StatusCode(500, result);
            }

            return Ok(result);
        }

        [HttpGet("LeaveRequestByUserId/{id}")]
        public async Task<IActionResult> GetLeaveByUserId(int id)
        {
            var leaves = await _leaveRequestService.GetLeaveRequestByUserId(id);

            return Ok(leaves);
        }

        [HttpGet("medical-certificate/{leaveRequestId}")]
        public async Task<IActionResult> GetMedicalCertificate(int leaveRequestId)
        {
            var leaveRequest = await _dataContext.LeaveRequests
                .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId);

            if (leaveRequest == null || string.IsNullOrEmpty(leaveRequest.MedicalCertificatePath))
                return NotFound("Medical certificate not found.");

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, leaveRequest.MedicalCertificatePath);

            if (!System.IO.File.Exists(fullPath))
                return NotFound("File not found on server.");

            var contentType = GetContentType(fullPath);
            var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

            return File(fileStream, contentType, leaveRequest.MedicalCertificateFileName);
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
            return Ok(result);
        }

        [HttpPost("CreateBulkOpinion")]
        public async Task<IActionResult> CreateBulkOpinion([FromBody] CreateBulkOpinionDto opinion) {
            // Validate the opinion object
            if (opinion == null)
            {
                return BadRequest("Opinion data is required.");
            }
            var result = await _leaveRequestService.GiveBulkOpinion(opinion);
            // Logic to create a new opinion
            return Ok(result);
        }
        // POST: api/Leave
        [HttpPost]
        public async Task<IActionResult> CreateLeave([FromForm] CreateLeaveRequestDto leave)
        {

            // Validate the leave object
            if (leave == null)
            {
                return BadRequest("Leave data is required.");
            }

            var result = await _leaveRequestService.CreateLeaveRequest(leave);  
            // Logic to create a new leave
            return Ok(result);
        }
        // PUT: api/Leave/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateLeave(int id, [FromBody] object leave)
        {
            // Logic to update leave by id
            return NoContent();
        }
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelLeaveRequest(int id)
        {
            var result = await _leaveRequestService.CancelleLeave(id);
            if (result.Error)
                return BadRequest(result);

            return Ok(result);
        }


        // DELETE: api/Leave/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteLeave(int id)
        {
            // Logic to delete leave by id
            return NoContent();
        }


        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out string contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }


    }
}
