using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Dtos.WorkFromHomeDtos;
using AutomatedTaskSystem.Services.Log;
using AutomatedTaskSystem.Services.WorkFromHome;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace AutomatedTaskSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WorkFromHomeController : ControllerBase
    {
        private readonly IWorkFromHomeService _workFromHomeService;
        private readonly DataContext _dataContext; // Often injected, but consider if directly needed in controller
        private readonly IWebHostEnvironment _webHostEnvironment; // Not strictly needed for WFH, but kept if you might add attachments
        private readonly ILogService _logService; // For logging within the controller

        public WorkFromHomeController(
            IWorkFromHomeService workFromHomeService,
            DataContext dataContext, // Consider if direct DataContext access is needed here or only in service
            IWebHostEnvironment webHostEnvironment, // Only if file handling is added later
            ILogService logService)
        {
            _workFromHomeService = workFromHomeService;
            _dataContext = dataContext; // If the controller needs direct DB access for something (e.g., getting file paths)
            _webHostEnvironment = webHostEnvironment; // If you plan to add file uploads/downloads for WFH requests
            _logService = logService;
        }

        // --- Create Work From Home Request ---
        [HttpPost] // Typically POST to the collection endpoint for creation
        public async Task<IActionResult> CreateWorkFromHome([FromBody] CreateWorkFromHomeDto request)
        {
            if (request == null)
            {
                _logService.LogWarning("CreateWorkFromHome: Received null request.");
                return BadRequest("Work from home request data is required.");
            }

            _logService.LogInformation("CreateWorkFromHome: Processing request for UserId: {UserId}, Date: {Date}", request.UserId, request.Date);
            var result = await _workFromHomeService.CreateWorkFromHomeRequest(request);

            if (result.Error)
            {
                _logService.LogError(null, "CreateWorkFromHome: Failed with error: {ErrorMessage}", result.Message);
                return BadRequest(result);
            }

            _logService.LogInformation("CreateWorkFromHome: Request processed successfully.");
            return Ok(result);
        }

        // --- Give Opinion on Work From Home Request ---
        // In WorkFromHomeController.cs
        [HttpPost("give-opinion")]
        public async Task<IActionResult> GiveWorkFromHomeOpinion([FromBody] CreateWorkFromHomeOpinionDto opinionDto) // Changed parameter type
        {
            if (opinionDto == null)
            {
                _logService.LogWarning("GiveWorkFromHomeOpinion: Received null opinion DTO.");
                return BadRequest("Opinion data is required.");
            }

            // Remove the validation for LeaveRequestId vs WorkFromHomeId as it's specific now
            // if ((opinionDto.LeaveRequestId.HasValue && opinionDto.WorkFromHomeId.HasValue) || ... )

            _logService.LogInformation("GiveWorkFromHomeOpinion: Processing opinion for WorkFromHomeId: {WorkFromHomeId}, IsApproved: {IsApproved}", opinionDto.WorkFromHomeId, opinionDto.IsApproved);
            var result = await _workFromHomeService.GiveWorkFromHomeOpinion(opinionDto);

            if (result.error)
            {
                _logService.LogError(null, "GiveWorkFromHomeOpinion: Failed with error: {ErrorMessage}", result.Message);
                return BadRequest(result);
            }

            _logService.LogInformation("GiveWorkFromHomeOpinion: Opinion processed successfully.");
            return Ok(result);
        }

        // --- Get All Work From Home Requests ---
        [HttpGet]
        public async Task<IActionResult> GetAllWorkFromHomeRequests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? fromDate = null,
            [FromQuery] string? toDate = null,
            [FromQuery] string? status = null,
            [FromQuery] string? myStatus = null,
            [FromQuery] bool disablePagination = false)
        {
            _logService.LogInformation("GetAllWorkFromHomeRequests: Fetching all WFH requests with filters. Page: {Page}, PageSize: {PageSize}", page, pageSize);
            var result = await _workFromHomeService.GetAllWorkFromHomeRequestsAsync(
                page, pageSize, searchTerm, fromDate, toDate, status, myStatus, disablePagination);

            if (result.Error)
            {
                _logService.LogError(null, "GetAllWorkFromHomeRequests: Failed with error: {ErrorMessage}", result.Message);
                return BadRequest(result);
            }

            _logService.LogInformation("GetAllWorkFromHomeRequests: Successfully retrieved {Count} WFH requests.", result.Data?.items?.Count);
            return Ok(result);
        }

        // --- Get Single Work From Home Request by ID ---
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkFromHomeById(int id)
        {
            _logService.LogInformation("GetWorkFromHomeById: Fetching WFH request with ID: {Id}", id);
            var result = await _workFromHomeService.GetWorkFromHomeRequestByIdAsync(id);

            if (result.Error)
            {
                _logService.LogWarning("GetWorkFromHomeById: WFH request with ID {Id} not found or error occurred: {ErrorMessage}", id, result.Message);
                // Return NotFound if explicitly not found, else BadRequest for other errors
                return NotFound(result);
            }

            _logService.LogInformation("GetWorkFromHomeById: Successfully retrieved WFH request with ID: {Id}", id);
            return Ok(result);
        }

        // --- Get Work From Home Requests by Specific User ID ---
        [HttpGet("user/{userId}")] // Renamed from "WorkFromHomeRequestsByUserId" for clarity and consistency
        public async Task<IActionResult> GetWorkFromHomeRequestsByUserId(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? fromDate = null,
            [FromQuery] string? toDate = null,
            [FromQuery] string? status = null,
            [FromQuery] bool disablePagination = false)
        {
            _logService.LogInformation("GetWorkFromHomeRequestsByUserId: Fetching WFH requests for User ID: {UserId}. Page: {Page}, PageSize: {PageSize}", userId, page, pageSize);
            var result = await _workFromHomeService.GetWorkFromHomeRequestsByUserId(
                userId, page, pageSize, searchTerm, fromDate, toDate, status, disablePagination);

            if (result.Error)
            {
                _logService.LogError(null, "GetWorkFromHomeRequestsByUserId: Failed for User ID {UserId} with error: {ErrorMessage}", userId, result.Message);
                return BadRequest(result);
            }

            _logService.LogInformation("GetWorkFromHomeRequestsByUserId: Successfully retrieved {Count} WFH requests for User ID: {UserId}", result.Data?.items?.Count, userId);
            return Ok(result);
        }

        // --- Cancel Work From Home Request ---
        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelWorkFromHome(int id)
        {
            _logService.LogInformation("CancelWorkFromHome: Attempting to cancel WFH request with ID: {Id}", id);
            var result = await _workFromHomeService.CancelWorkFromHome(id);

            if (result.Error)
            {
                _logService.LogError(null, "CancelWorkFromHome: Failed to cancel WFH request {Id} with error: {ErrorMessage}", id, result.Message);
                return BadRequest(result);
            }

            _logService.LogInformation("CancelWorkFromHome: WFH request {Id} cancelled successfully.", id);
            return Ok(result);
        }

        // --- Placeholder for Delete (if needed) ---
        // Work From Home requests might be better managed by status changes (Approved, Rejected, Cancelled)
        // rather than hard deletion, but including for completeness if your business logic requires it.
        [HttpDelete("{id}")]
        public IActionResult DeleteWorkFromHome(int id)
        {
            _logService.LogWarning("DeleteWorkFromHome: Direct deletion of WFH request {Id} requested. Consider if status change is more appropriate.", id);
            // Example: var result = await _workFromHomeService.DeleteWorkFromHomeAsync(id);
            // if (!result) return NotFound("Work from home request not found for deletion.");
            // return NoContent(); // Or return a ResponseService result
            return StatusCode(StatusCodes.Status501NotImplemented, "Direct deletion of Work From Home requests is not implemented or recommended. Use 'cancel' instead.");
        }

        // --- Helper for Content Type (if you add file attachments to WFH in future) ---
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
