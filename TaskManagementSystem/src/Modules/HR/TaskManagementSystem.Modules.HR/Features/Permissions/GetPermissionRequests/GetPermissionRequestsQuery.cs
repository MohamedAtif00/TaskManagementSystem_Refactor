using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.GetPermissionRequests;

public sealed record GetPermissionRequestsQuery(int UserId) : IQuery<Result<IReadOnlyList<PermissionRequestResult>>>;

