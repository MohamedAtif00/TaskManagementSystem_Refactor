using AutomatedTaskSystem.Dtos.LeaveDtos;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.Leave
{
    public interface ILeaveRequestService
    {
        Task<bool> CreateLeaveRequest(CreateLeaveRequestDto request);

        //Task<bool> AddLeaveRequest(CreateLeaveRequestDto[] request, int userId);
        Task<bool> DeleteVacationAsync(int id);
        Task<List<GetOpinion>> GetAllOpinionsForLeaveRequest(int leaveRequestId);
        Task<ResponseService<List<GetLeaveRequestDto>>> GetAllVacationsAsync(int? userId = null,int? role = null);
        Task<ResponseService<List<GetLeaveRequestDto>>> GetLeaveRequestsByUserId(int userId);
        Task<ResponseService<GetSingleLeaveRequestDto>> GetLeaveRequestByUserId(int userId);
        Task<ActionResult<ResponseService<GetLeaveRequestDto>>> GetVacationByIdAsync(int id);
        Task<bool> UpdateVacationAsync(int id, DateTime startDate, DateTime endDate, string? reason, LeaveRequestStatusEnum status);
        Task<bool> GiveOpinion(CreateOpinionDto request);
        Task<ResponseService<GetOpinion>> GetSingleOpinion(int id);
    }
}