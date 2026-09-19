using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.Modules.HR.Features.Permissions.GivePermissionOpinion;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.ApprovePermissionRequest;

public sealed record ApprovePermissionRequestCommand(int ActorUserId, int PermissionId)
    : ICommand<Result<PermissionRequestResult>>;

