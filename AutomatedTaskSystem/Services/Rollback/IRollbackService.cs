using AutomatedTaskSystem.Dtos.Tasks;
using AutomatedTaskSystem.Services.ResponseService;

namespace AutomatedTaskSystem.Services.RollbackService;

public interface IRollbackService
{
    Task<BaseResponseService> CreateRollback(
        int FromTaskId,
        int ToTaskId,
        int UserId,
        string? Clarification,
		List<RollbackLogDto> logs
    );
}
