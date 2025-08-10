using System.Threading.Tasks;
using AutomatedTaskSystem.Dtos.PermissionDtos;
using AutomatedTaskSystem.Services.Permission;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        // GET: api/Permission
        [HttpGet]
        public async Task<IActionResult> GetAllPermissions(
                [FromQuery] int? userId,
                [FromQuery] int? role,
                [FromQuery] int page = 1, // Add pagination parameter
                [FromQuery] int pageSize = 10, // Add pagination parameter
                [FromQuery] string? searchTerm = null, // Add search term parameter
                [FromQuery] string? date = null, // Add filter for specific date
                [FromQuery] string? status = null, // Add filter for status
                [FromQuery] string? myStatus = null, // Add filter for status
                [FromQuery] string? type = null // Add filter for type
            )
        {
            // Pass all the received query parameters to the service method
            var result = await _permissionService.GetAllPermissionsAsync(
                userId,
                role,
                page,
                pageSize,
                searchTerm,
                date,
                status,
                myStatus,
                type
            );

            // Check if the service method returned an error
            if (result.Error)
            {
                return BadRequest(result); // Return 400 Bad Request if there's an error
            }

            // Return 200 OK with the paginated data
            return Ok(result);
        }

        // GET: api/Permission/{id}
        [HttpGet("GetPermissionsByUserId/{userId}")]
        public async Task<IActionResult> GetPermissionsByUserId(
                int userId,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10,
                [FromQuery] string? searchTerm = null,
                [FromQuery] string? fromDate = null,
                [FromQuery] string? toDate = null,
                [FromQuery] string? status = null,
                [FromQuery] string? type = null,
                [FromQuery] bool disablePagination = false)
        {
            var result = await _permissionService.GetPermissionsByUserId(
                userId,
                page,
                pageSize,
                searchTerm,
                fromDate,
                toDate,
                status,
                type,
                disablePagination);

            if (result.Error)
            {
                return StatusCode(500, result);
            }

            return Ok(result);
        }

        // GET: api/Permission/PermissionsByUserId/{id}
        //[HttpGet("PermissionsByUserId/{id}")]
        //public async Task<IActionResult> GetPermissionsByUserId(int id)
        //{
        //    var result = await _permissionService.GetPermissionsByUserIdAsync(id);
        //    return Ok(result);
        //}

        // GET: api/Permission/GetSinglePermission/{id}
        [HttpGet("GetSinglePermission/{id}")]
        public async Task<IActionResult> GetSinglePermission(int id)
        {
            var result = await _permissionService.GetPermissionDetailByIdAsync(id);
            if (result == null || result.Data == null)
                return NotFound(new { error = true, message = "Permission not found" });
            return Ok(result);
        }

        // POST: api/Permission
        [HttpPost]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto request)
        {
            if (request == null)
            {
                return BadRequest("Permission data is required.");
            }

            var success = await _permissionService.CreatePermissionRequest(request);

            return Ok(success);
        }

        // PUT: api/Permission/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] UpdatePermissionDto request)
        {
            if (request == null)
            {
                return BadRequest("Permission data is required.");
            }

            request.Id = id; // Ensure ID matches route
            var success = await _permissionService.UpdatePermissionAsync(request);
            if (!success)
                return BadRequest(new { error = true, message = "Could not update permission" });

            return NoContent();
        }

        [HttpPut("cancel/{id}")]
        public async Task<IActionResult> CancelPermission(int id)
        {
            var result = await _permissionService.CancelPermissionAsync(id);
            if (result.Error)
                return BadRequest(result);

            return Ok(result);
        }


        // DELETE: api/Permission/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var success = await _permissionService.DeletePermissionAsync(id);
            if (!success)
                return NotFound(new { error = true, message = "Permission not found" });

            return NoContent();
        }

        // POST: api/Permission/Approve
        [HttpPost("Approve")]
        public async Task<IActionResult> ApprovePermission([FromBody] ApprovePermissionDto request)
        {
            if (request == null)
            {
                return BadRequest("Approval data is required.");
            }

            var success = await _permissionService.ApproveOrRejectPermissionAsync(
                request.PermissionId,
                request.IsApproved,
                request.Comment);

            //if (!success)
            //    return BadRequest(new { error = true, message = "Could not process approval" });

            return Ok(new { error = false, message = "Permission status updated" });
        }

        // POST: api/Permission/Approve
        [HttpPost("BulkApprove")]
        public async Task<IActionResult> BulkApprovePermission([FromBody] CreateBulkPermissionOpinionDto request)
        {
            if (request == null)
            {
                return BadRequest("Approval data is required.");
            }

            var success = await _permissionService.GiveBulkPermissionOpinion(request);

            //if (!success)
            //    return BadRequest(new { error = true, message = "Could not process approval" });

            return Ok(new { error = false, message = "Permission status updated" });
        }
    }
}