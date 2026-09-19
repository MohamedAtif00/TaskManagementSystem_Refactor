using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.GivePermissionOpinion;

public sealed record GivePermissionOpinionCommand(
    int ActorUserId,
    string ActorRole,
    int PermissionId,
    bool IsApproved,
    string? Comment) : ICommand<Result<PermissionRequestResult>>;

