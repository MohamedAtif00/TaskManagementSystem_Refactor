using AutomatedTaskSystem.Dtos.PermissionDtos;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.Permission;
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

        // POST: api/Permission
        [HttpPost]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto dto)
        {
            var success = await _permissionService.AddPermissionAsync(dto);
            if (!success)
                return BadRequest(new { error = true, message = "Could not create permission (limit exceeded or user not found)." });

            return Ok(new { error = false, message = "Permission created successfully.",data=success });
        }

        // GET: api/Permission
        [HttpGet]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _permissionService.GetAllPermissionsAsync();
            return Ok(new { error = false, data = permissions });
        }

        // GET: api/Permission/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermissionById(int id)
        {
            var permission = await _permissionService.GetPermissionByIdAsync(id);
            if (permission == null)
                return NotFound(new { error = true, message = "Permission not found." });

            return Ok(new { error = false, data = permission });
        }

        // PUT: api/Permission
        [HttpPut]
        public async Task<IActionResult> UpdatePermission([FromBody] UpdatePermissionDto dto)
        {
            var success = await _permissionService.UpdatePermissionAsync(dto);
            if (!success)
                return BadRequest(new { error = true, message = "Could not update permission (not found or internal error)." });

            return Ok(new { error = false, message = "Permission updated successfully." });
        }

        // DELETE: api/Permission/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var success = await _permissionService.DeletePermissionAsync(id);
            if (!success)
                return NotFound(new { error = true, message = "Permission not found." });

            return Ok(new { error = false, message = "Permission deleted successfully." });
        }
    }
}
