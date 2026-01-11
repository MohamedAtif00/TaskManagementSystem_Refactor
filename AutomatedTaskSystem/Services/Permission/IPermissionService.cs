using AutomatedTaskSystem.Dtos.PermissionDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Services.Leave;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.Permission
{
    public interface IPermissionService
    {
        Task<LeaveRequestService.OperationResult> ApproveOrRejectPermissionAsync(int id, bool isApproved, string? comment);

        //Task<bool> ApproveOrRejectPermissionAsync(int id, bool isApproved, string comment);
        Task<ResponseService<bool>> CancelPermissionAsync(int permissionId);

        Task<ResponseService<bool>> CreatePermissionRequest(CreatePermissionDto request);
        Task<bool> DeletePermissionAsync(int id);
        Task<ResponseService<PageList<GetPermissionDto>>> GetAllPermissionsAsync(int? userId = null, int? role = null, int page = 1, int pageSize = 10, string? searchTerm = null, string? date = null, string? status = null, string? myStatus = null, string? type = null);
        Task<ResponseService<PageList<GetPermissionDto>>> GetPermissionByIdAsync(int userId, int page, int pageSize);
        Task<ResponseService<GetSinglePermissionDto>> GetPermissionDetailByIdAsync(int permissionId);
        Task<ResponseService<PageList<GetPermissionDto>>> GetPermissionsByUserId(int userId, int page, int pageSize, string? searchTerm, string? fromDate, string? toDate, string? status, string? type, bool disablePagination);
        Task<LeaveRequestService.OperationResult> GiveBulkPermissionOpinion(CreateBulkPermissionOpinionDto request);

        //Task<ResponseService<List<GetPermissionDto>>> GetPermissionsByUserIdAsync(int userId);
        Task<bool> UpdatePermissionAsync(UpdatePermissionDto request);
    }
}