using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Permissions;

public sealed record ListPermissionsQuery : IQuery<Result<IReadOnlyList<PermissionDto>>>;
