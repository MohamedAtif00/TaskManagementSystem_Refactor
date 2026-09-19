using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.GiveBulkForgotClockOpinion;

public sealed record GiveBulkForgotClockOpinionCommand(
    int ActorUserId,
    string ActorRole,
    IReadOnlyList<int> ForgotClockRequestIds,
    bool IsApproved,
    string? Comment) : IHrCommand<Result<BulkForgotClockOpinionResult>>;

