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
        Task<ActionResult<ResponseService<List<GetLeaveRequestDto>>>> GetAllVacationsAsync(int? userId = null);
        Task<ActionResult<ResponseService<GetLeaveRequestDto>>> GetVacationByIdAsync(int id);
        Task<bool> UpdateVacationAsync(int id, DateTime startDate, DateTime endDate, string? reason, LeaveRequestStatusEnum status);
    }
}