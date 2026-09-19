using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.RequestPermission;

public sealed record RequestPermissionCommand(
    int UserId,
    string RequesterRole,
    PermissionType Type,
    DateTime PermissionDate,
    TimeOnly FromTime,
    TimeOnly ToTime,
    string? Reason) : ICommand<Result<PermissionRequestResult>>;

