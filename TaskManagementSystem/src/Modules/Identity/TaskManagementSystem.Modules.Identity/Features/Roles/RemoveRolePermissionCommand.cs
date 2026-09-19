using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed record RemoveRolePermissionCommand(int RoleId, int PermissionId) : ICommand<Result<RoleDto>>;
