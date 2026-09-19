using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.SearchLeaveRequests;

public sealed record SearchLeaveRequestsQuery(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId,
    int Page,
    int PageSize,
    string? Search,
    DateTime? FromDate,
    DateTime? ToDate,
    LeaveStatus? Status,
    LeaveType? Type,
    string? MyStatus) : IQuery<Result<LeaveRequestListResult>>;

