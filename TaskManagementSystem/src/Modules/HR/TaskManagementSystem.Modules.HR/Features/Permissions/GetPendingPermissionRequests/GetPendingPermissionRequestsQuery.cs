using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.GetPendingPermissionRequests;

public sealed record GetPendingPermissionRequestsQuery(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId) : IQuery<Result<IReadOnlyList<PermissionRequestResult>>>;

