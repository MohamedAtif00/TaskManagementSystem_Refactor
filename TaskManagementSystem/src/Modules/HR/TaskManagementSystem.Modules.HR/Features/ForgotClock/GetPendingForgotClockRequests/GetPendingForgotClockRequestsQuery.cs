using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.GetPendingForgotClockRequests;

public sealed record GetPendingForgotClockRequestsQuery(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId) : IQuery<Result<IReadOnlyList<ForgotClockRequestResult>>>;

