using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.GiveForgotClockOpinion;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.ApproveForgotClockRequest;

public sealed record ApproveForgotClockRequestCommand(int ActorUserId, int ForgotClockRequestId)
    : ICommand<Result<ForgotClockRequestResult>>;

