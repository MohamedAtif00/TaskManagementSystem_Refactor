using MediatR;
using Microsoft.Extensions.Options;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GetLeaveSettings;

public sealed record GetLeaveSettingsQuery : IQuery<Result<LeaveSettingsResult>>;

public sealed record LeaveSettingsResult(
    int FromNextBalanceMaxDays,
    string? FromNextBalanceStartDate,
    string? FromNextBalanceEndDate,
    string? EmergencyBlackoutCutoffDate,
    string? ResetDate,
    bool EmergencyAllowedToday,
    bool FromNextWindowActiveToday);

public sealed class GetLeaveSettingsQueryHandler(
    IOptions<LeaveSettingsOptions> leaveSettings,
    TimeProvider timeProvider)
    : IRequestHandler<GetLeaveSettingsQuery, Result<LeaveSettingsResult>>
{
    public Task<Result<LeaveSettingsResult>> Handle(
        GetLeaveSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = leaveSettings.Value;
        var today = timeProvider.GetUtcNow().UtcDateTime.Date;

        return Task.FromResult(Result.Ok(new LeaveSettingsResult(
            settings.FromNextBalanceMaxDays,
            settings.FromNextBalanceStartDate,
            settings.FromNextBalanceEndDate,
            settings.EmergencyBlackoutCutoffDate,
            settings.ResetDate,
            LeaveSettingsHelper.IsEmergencyAllowed(settings, today),
            LeaveSettingsHelper.IsInFromNextWindow(settings, today))));
    }
}
