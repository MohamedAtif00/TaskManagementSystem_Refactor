using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    IReadOnlyList<int> PermissionIds) : ICommand<Result<RoleDto>>;
