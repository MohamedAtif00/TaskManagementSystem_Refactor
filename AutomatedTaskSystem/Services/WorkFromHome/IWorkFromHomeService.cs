using AutomatedTaskSystem.Dtos.WorkFromHomeDtos;
using AutomatedTaskSystem.Helper;
using AutomatedTaskSystem.Services.Leave;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.WorkFromHome
{
    public interface IWorkFromHomeService
    {
        Task<ResponseService<bool>> CancelWorkFromHome(int id);
        Task<ResponseService<bool>> CreateWorkFromHomeRequest(CreateWorkFromHomeDto request);
        Task<ResponseService<PageList<GetWorkFromHomeDto>>> GetAllWorkFromHomeRequestsAsync(int page = 1, int pageSize = 10, string? searchTerm = null, string? fromDate = null, string? toDate = null, string? status = null, string? myStatus = null, bool disablePagination = false);
        Task<ResponseService<GetSingleWorkFromHomeDto>> GetWorkFromHomeRequestByIdAsync(int id);
        Task<ResponseService<PageList<GetWorkFromHomeDto>>> GetWorkFromHomeRequestsByUserId(int userId, int page, int pageSize, string? searchTerm, string? fromDate, string? toDate, string? status, bool disablePagination);
        Task<LeaveRequestService.OperationResult> GiveWorkFromHomeOpinion(CreateWorkFromHomeOpinionDto request);
    }
}