using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.SearchWorkFromHomeRequests;

public sealed record SearchWorkFromHomeRequestsQuery(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId,
    int Page,
    int PageSize,
    string? Search,
    DateTime? FromDate,
    DateTime? ToDate,
    WorkFromHomeStatus? Status,
    string? MyStatus) : IQuery<Result<WorkFromHomeRequestListResult>>;

