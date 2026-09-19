using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;

namespace TaskManagementSystem.Api.Endpoints.HR.ForgotClock;

internal static class ForgotClockMapping
{
    internal static ForgotClockPunchType? ParsePunchType(string? value) =>
        ForgotClockPunchTypeMapping.Parse(value);

    internal static ForgotClockStatus? ParseForgotClockStatus(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : Enum.TryParse<ForgotClockStatus>(value, true, out var status)
                ? status
                : null;

    internal static ForgotClockRequestResponse MapForgotClock(ForgotClockRequestResult request) =>
        new()
        {
            Id = request.Id,
            UserId = request.UserId,
            PunchType = request.PunchType.ToString(),
            Status = request.Status.ToString(),
            AttendanceDate = request.AttendanceDate,
            IntendedTime = request.IntendedTime.ToString("HH:mm:ss"),
            Reason = request.Reason,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            Opinions = request.Opinions?.Select(opinion => new OpinionResponse
            {
                Id = opinion.Id,
                UserId = opinion.UserId,
                IsApproved = opinion.IsApproved,
                Comment = opinion.Comment,
                CreatedAt = opinion.CreatedAt
            }).ToList()
        };
}
