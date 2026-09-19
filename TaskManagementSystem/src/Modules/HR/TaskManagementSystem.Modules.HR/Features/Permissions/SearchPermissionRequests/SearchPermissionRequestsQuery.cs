using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.SearchPermissionRequests;

public sealed record SearchPermissionRequestsQuery(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId,
    int Page,
    int PageSize,
    string? Search,
    DateTime? Date,
    DateTime? FromDate,
    DateTime? ToDate,
    PermissionStatus? Status,
    PermissionType? Type,
    string? MyStatus) : IQuery<Result<PermissionRequestListResult>>;

