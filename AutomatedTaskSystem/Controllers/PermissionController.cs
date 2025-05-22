using System.Threading.Tasks;
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
        public async Task<IActionResult> GetAllPermissions([FromQuery] int? userId, [FromQuery] int? role)
        {
            var result = await _permissionService.GetAllPermissionsAsync(userId, role);
            return Ok(result);
        }

        // GET: api/Permission/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            var result = await _permissionService.GetPermissionByIdAsync(id);
            if (result == null)
                return NotFound(new { error = true, message = "Permission not found" });
            return Ok(result);
        }

        // GET: api/Permission/PermissionsByUserId/{id}
        [HttpGet("PermissionsByUserId/{id}")]
        public async Task<IActionResult> GetPermissionsByUserId(int id)
        {
            var result = await _permissionService.GetPermissionsByUserIdAsync(id);
            return Ok(result);
        }

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
            if (!success)
                return BadRequest(new { error = true, message = "Could not create permission" });

            return CreatedAtAction(nameof(GetPermissionById), new { id = 0 }, request);
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

            if (!success)
                return BadRequest(new { error = true, message = "Could not process approval" });

            return Ok(new { error = false, message = "Permission status updated" });
        }
    }
}