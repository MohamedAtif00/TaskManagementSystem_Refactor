using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.Leave
{
    public interface ILeaveRequestService
    {
        Task<ResponseService<bool>> CreateLeaveRequest(CreateLeaveRequestDto request);

        //Task<bool> AddLeaveRequest(CreateLeaveRequestDto[] request, int userId);
        Task<bool> DeleteVacationAsync(int id);
        Task<List<GetOpinion>> GetAllOpinionsForLeaveRequest(int leaveRequestId);
        Task<ResponseService<GetSingleLeaveRequestDto>> GetLeaveRequestByUserId(int userId);
        Task<ActionResult<ResponseService<GetLeaveRequestDto>>> GetVacationByIdAsync(int id);
        Task<bool> UpdateVacationAsync(int id, DateTime startDate, DateTime endDate, string? reason, LeaveRequestStatusEnum status);
        //Task<bool> GiveOpinion(CreateOpinionDto request);
        Task<ResponseService<GetOpinion>> GetSingleOpinion(int id);
        Task<ResponseService<bool>> CancelleLeave(int id);
        Task<ResponseService<PageList<GetLeaveRequestDto>>> GetAllVacationsAsync( int page = 1, int pageSize = 10, string? searchTerm = null, string? fromDate = null, string? toDate = null, string? status = null,string? myStatus = null, string? type = null, bool disablePagination = false);
        Task<ResponseService<PageList<GetLeaveRequestDto>>> GetLeaveRequestsByUserId(int userId, int page, int pageSize, string? searchTerm, string? fromDate, string? toDate, string? status, string? type, bool disablePagination);
        Task<LeaveRequestService.OperationResult> GiveOpinion(CreateOpinionDto request);
        Task<LeaveRequestService.OperationResult> GiveBulkOpinion(CreateBulkOpinionDto request);
        Task<ResponseService<LeavePreviewDto>> PreviewAnnualLeave(int userId, string startDate, string endDate);
    }
}