using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GetPendingLeaveRequests;

public sealed record GetPendingLeaveRequestsQuery(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId) : IQuery<Result<IReadOnlyList<LeaveRequestResult>>>;

