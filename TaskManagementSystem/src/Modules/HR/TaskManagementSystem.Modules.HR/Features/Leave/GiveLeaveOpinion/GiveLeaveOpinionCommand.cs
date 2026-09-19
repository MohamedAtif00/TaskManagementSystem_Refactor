using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GiveLeaveOpinion;

public sealed record GiveLeaveOpinionCommand(
    int ActorUserId,
    string ActorRole,
    int LeaveRequestId,
    bool IsApproved,
    string? Comment) : ICommand<Result<LeaveRequestResult>>;

