using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.Permission
{
    public interface IPermissionService
    {
        // Create a new permission request
        Task<bool> CreatePermissionRequest(CreatePermissionDto request);

        // Get all permissions with optional filter by user and role
        Task<ResponseService<List<GetPermissionDto>>> GetAllPermissionsAsync(int? userId = null, int? role = null);

        // Get a single permission by ID
        Task<ActionResult<ResponseService<GetPermissionDto>>> GetPermissionByIdAsync(int id);

        // Get a detailed permission by ID
        Task<ResponseService<GetSinglePermissionDto>> GetPermissionDetailByIdAsync(int permissionId);

        // Get all permissions for a specific user
        Task<ResponseService<List<GetPermissionDto>>> GetPermissionsByUserIdAsync(int userId);

        // Update a permission
        Task<bool> UpdatePermissionAsync(UpdatePermissionDto request);

        // Delete a permission
        Task<bool> DeletePermissionAsync(int id);

        // Approve or reject a permission
        Task<bool> ApproveOrRejectPermissionAsync(int id, bool isApproved, string comment);
    }
}