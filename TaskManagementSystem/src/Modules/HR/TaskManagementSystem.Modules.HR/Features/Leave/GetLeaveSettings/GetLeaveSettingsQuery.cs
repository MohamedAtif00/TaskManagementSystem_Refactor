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
