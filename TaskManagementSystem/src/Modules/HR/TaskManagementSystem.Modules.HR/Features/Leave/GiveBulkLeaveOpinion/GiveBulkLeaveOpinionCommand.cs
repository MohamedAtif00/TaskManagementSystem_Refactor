using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GiveBulkLeaveOpinion;

public sealed record GiveBulkLeaveOpinionCommand(
    int ActorUserId,
    string ActorRole,
    IReadOnlyList<int> LeaveRequestIds,
    bool IsApproved,
    string? Comment) : IHrCommand<Result<BulkLeaveOpinionResult>>;

public sealed record BulkLeaveOpinionResult(
    int Succeeded,
    int Failed,
    IReadOnlyList<int> FailedLeaveRequestIds);
