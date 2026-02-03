using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Services.RollbackService;

public interface IRollbackService
{
    Task<ActionResult<ResponseService<GetRollbackHistoryDto>>> GetRollbackHistory(int id);
    Task<BaseResponseService> CreateRollback(
        int FromTaskId,
        int ToTaskId,
        User user,
        string? Clarification,
        List<RollbackLogDto> logs
    );
}
