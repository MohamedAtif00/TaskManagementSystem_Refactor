using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.SearchForgotClockRequests;

public sealed record SearchForgotClockRequestsQuery(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId,
    int Page,
    int PageSize,
    string? Search,
    DateTime? Date,
    DateTime? FromDate,
    DateTime? ToDate,
    ForgotClockStatus? Status,
    ForgotClockPunchType? PunchType,
    string? MyStatus) : IQuery<Result<ForgotClockRequestListResult>>;

