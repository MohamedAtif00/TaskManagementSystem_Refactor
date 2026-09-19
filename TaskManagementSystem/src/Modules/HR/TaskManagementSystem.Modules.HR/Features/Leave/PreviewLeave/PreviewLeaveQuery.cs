using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure;

namespace TaskManagementSystem.Modules.HR.Features.Leave.PreviewLeave;

public sealed record PreviewLeaveQuery(
    int UserId,
    DateTime StartDate,
    DateTime EndDate) : IQuery<Result<PreviewLeaveResult>>;

public sealed record PreviewLeaveResult(
    int RequestedDays,
    int AvailableAnnual,
    int NeededFromNext,
    int FromNextBalanceMaxDays,
    int AlreadyUsedFromNext,
    int PendingFromNext,
    bool RequiresConfirmation,
    string? ErrorMessage);
