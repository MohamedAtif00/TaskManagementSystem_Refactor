using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed record ListRolesQuery : IQuery<Result<IReadOnlyList<RoleDto>>>;
