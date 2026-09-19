using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.CancelLeaveRequest;

public sealed record CancelLeaveRequestCommand(int UserId, int LeaveRequestId)
    : ICommand<Result<LeaveRequestResult>>, IHrCommand<Result<LeaveRequestResult>>;

