using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GetPendingWorkFromHomeRequests;

public sealed record GetPendingWorkFromHomeRequestsQuery(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId) : IQuery<Result<IReadOnlyList<WorkFromHomeRequestResult>>>;

