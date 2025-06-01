using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.Permission
{
    public interface IPermissionService
    {
        Task<bool> ApproveOrRejectPermissionAsync(int id, bool isApproved, string comment);
        Task<ResponseService<bool>> CancelPermissionAsync(int permissionId);

        //// Create a new permission request
        //Task<ResponseService<bool>> CreatePermissionRequest(CreatePermissionDto request);

        //// Get all permissions with optional filter by user and role

        //// Get a single permission by ID
        //Task<ActionResult<ResponseService<GetPermissionDto>>> GetPermissionByIdAsync(int id);

        //// Get a detailed permission by ID
        //Task<ResponseService<GetSinglePermissionDto>> GetPermissionDetailByIdAsync(int permissionId);

        //// Get all permissions for a specific user
        ////Task<ResponseService<List<GetPermissionDto>>> GetPermissionsByUserIdAsync(int userId);

        //// Update a permission
        //Task<bool> UpdatePermissionAsync(UpdatePermissionDto request);

        //// Delete a permission
        //Task<bool> DeletePermissionAsync(int id);

        //// Approve or reject a permission
        //Task<bool> ApproveOrRejectPermissionAsync(int id, bool isApproved, string comment);
        //Task<ResponseService<bool>> CancelPermissionAsync(int permissionId);
        //Task<ResponseService<PageList<GetPermissionDto>>> GetAllPermissionsAsync(int? userId = null, int? role = null, int page = 1, int pageSize = 10, string? searchTerm = null, string? date = null, string? status = null, string? type = null);
        //Task<ResponseService<PageList<GetPermissionDto>>> GetPermissionsByUserIdAsync(int userId, int page, int pageSize);
        Task<ResponseService<bool>> CreatePermissionRequest(CreatePermissionDto request);
        Task<bool> DeletePermissionAsync(int id);
        Task<ResponseService<PageList<GetPermissionDto>>> GetAllPermissionsAsync(int? userId = null, int? role = null, int page = 1, int pageSize = 10, string? searchTerm = null, string? date = null, string? status = null, string? type = null);
        Task<ResponseService<PageList<GetPermissionDto>>> GetPermissionByIdAsync(int userId, int page, int pageSize);
        Task<ResponseService<GetSinglePermissionDto>> GetPermissionDetailByIdAsync(int permissionId);
        Task<ResponseService<List<GetPermissionDto>>> GetPermissionsByUserIdAsync(int userId);
        Task<bool> UpdatePermissionAsync(UpdatePermissionDto request);
    }
}