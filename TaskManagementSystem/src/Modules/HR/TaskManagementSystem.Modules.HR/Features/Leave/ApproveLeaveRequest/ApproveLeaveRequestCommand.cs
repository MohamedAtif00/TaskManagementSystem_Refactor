using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Features.Leave.GiveLeaveOpinion;

namespace TaskManagementSystem.Modules.HR.Features.Leave.ApproveLeaveRequest;

public sealed record ApproveLeaveRequestCommand(int ActorUserId, string ActorRole, int LeaveRequestId)
    : ICommand<Result<LeaveRequestResult>>;

