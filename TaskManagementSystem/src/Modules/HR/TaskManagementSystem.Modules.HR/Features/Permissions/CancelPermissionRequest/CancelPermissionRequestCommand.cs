using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.CancelPermissionRequest;

public sealed record CancelPermissionRequestCommand(int UserId, int PermissionId)
    : ICommand<Result<PermissionRequestResult>>;

