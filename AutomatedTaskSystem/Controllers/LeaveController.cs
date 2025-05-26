using System.Threading.Tasks;
using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.Dtos.LeaveDtos;
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
