using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.GetPermissionRequestById;

public sealed record GetPermissionRequestByIdQuery(int UserId, string UserRole, int PermissionId)
    : IQuery<Result<PermissionRequestResult>>;

