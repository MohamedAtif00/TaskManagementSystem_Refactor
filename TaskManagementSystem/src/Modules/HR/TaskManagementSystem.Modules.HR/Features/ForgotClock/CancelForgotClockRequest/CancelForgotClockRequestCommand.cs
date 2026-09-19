using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.CancelForgotClockRequest;

public sealed record CancelForgotClockRequestCommand(int UserId, int ForgotClockRequestId)
    : ICommand<Result<ForgotClockRequestResult>>;

