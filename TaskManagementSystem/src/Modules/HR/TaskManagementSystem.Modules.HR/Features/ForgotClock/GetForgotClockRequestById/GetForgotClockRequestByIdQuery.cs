using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.GetForgotClockRequestById;

public sealed record GetForgotClockRequestByIdQuery(int UserId, string UserRole, int ForgotClockRequestId)
    : IQuery<Result<ForgotClockRequestResult>>;

