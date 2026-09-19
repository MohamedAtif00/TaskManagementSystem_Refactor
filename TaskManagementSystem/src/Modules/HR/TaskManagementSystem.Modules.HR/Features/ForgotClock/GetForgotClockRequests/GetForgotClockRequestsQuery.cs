using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.GetForgotClockRequests;

public sealed record GetForgotClockRequestsQuery(int UserId)
    : IQuery<Result<IReadOnlyList<ForgotClockRequestResult>>>;

