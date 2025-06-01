using System.Threading.Tasks;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Services.Leave;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Hosting;
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

        public LeaveController(ILeaveRequestService leaveRequestService, DataContext dataContext, IWebHostEnvironment webHostEnvironment)
        {
            _leaveRequestService = leaveRequestService;
            _dataContext = dataContext;
            _webHostEnvironment = webHostEnvironment;
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
            [FromQuery] string? type = null, // Add filter for type
            [FromQuery] bool disablePagination = false // Add filter for type
        )
        {
            // Pass all the received query parameters to the service method
            var result = await _leaveRequestService.GetAllVacationsAsync(
                userId,
                role,
                page,
                pageSize,
                searchTerm,
                fromDate,
                toDate,
                status,
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
