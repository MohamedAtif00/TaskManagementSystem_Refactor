using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed record UpdateRoleCommand(int RoleId, string Name, string? Description)
    : ICommand<Result<RoleDto>>;
