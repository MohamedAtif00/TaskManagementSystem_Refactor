using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.GiveForgotClockOpinion;

public sealed record GiveForgotClockOpinionCommand(
    int ActorUserId,
    string ActorRole,
    int ForgotClockRequestId,
    bool IsApproved,
    string? Comment) : ICommand<Result<ForgotClockRequestResult>>;

