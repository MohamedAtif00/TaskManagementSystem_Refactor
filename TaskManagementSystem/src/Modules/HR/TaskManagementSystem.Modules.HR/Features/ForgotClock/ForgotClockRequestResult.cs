using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock;

public sealed record ForgotClockRequestResult(
    int Id,
    int UserId,
    ForgotClockPunchType PunchType,
    ForgotClockStatus Status,
    DateTime AttendanceDate,
    TimeOnly IntendedTime,
    string? Reason,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<OpinionResult>? Opinions = null)
{
    public static ForgotClockRequestResult From(
        ForgotClockRequest request,
        IReadOnlyList<OpinionResult>? opinions = null) =>
        new(
            request.Id,
            request.UserId,
            request.PunchType,
            request.Status,
            request.AttendanceDate,
            request.IntendedTime,
            request.Reason,
            request.CreatedAt,
            request.UpdatedAt,
            opinions);
}

public sealed record ForgotClockRequestListResult(
    IReadOnlyList<ForgotClockRequestResult> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record BulkForgotClockOpinionResult(
    int Succeeded,
    int Failed,
    IReadOnlyList<int> FailedForgotClockRequestIds);
