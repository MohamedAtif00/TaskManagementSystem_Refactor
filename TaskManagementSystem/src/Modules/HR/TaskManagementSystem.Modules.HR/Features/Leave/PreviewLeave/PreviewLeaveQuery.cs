using MediatR;
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

public sealed class PreviewLeaveQueryHandler(
    IHrUnitOfWork unitOfWork,
    LeaveRequestPlanner planner,
    IWorkingDayCalculator workingDayCalculator,
    TimeProvider timeProvider)
    : IRequestHandler<PreviewLeaveQuery, Result<PreviewLeaveResult>>
{
    public async Task<Result<PreviewLeaveResult>> Handle(
        PreviewLeaveQuery request,
        CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.EmployeeBalances.GetByUserIdAsync(request.UserId, cancellationToken);
        if (balance is null)
        {
            return Result.Fail<PreviewLeaveResult>(HrErrors.UserNotFound);
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var workingDays = await workingDayCalculator.CountAsync(request.StartDate, request.EndDate, cancellationToken);
        var pendingAnnual = await unitOfWork.LeaveRequests.SumPendingWorkingDaysAsync(
            request.UserId,
            LeaveType.Annual,
            cancellationToken: cancellationToken);
        var pendingFromNext = await unitOfWork.LeaveRequests.SumPendingWorkingDaysAsync(
            request.UserId,
            LeaveType.FromNextBalance,
            cancellationToken: cancellationToken);

        var availableAnnual = balance.AvailableAnnualLeave(pendingAnnual);
        var neededFromNext = Math.Max(0, workingDays - availableAnnual);
        var settings = planner.Settings;

        if (neededFromNext == 0)
        {
            return Result.Ok(new PreviewLeaveResult(
                workingDays,
                availableAnnual,
                0,
                settings.FromNextBalanceMaxDays,
                balance.FromNextBalanceDaysUsed,
                pendingFromNext,
                false,
                null));
        }

        if (!LeaveSettingsHelper.IsInFromNextWindow(settings, utcNow.Date))
        {
            return Result.Ok(new PreviewLeaveResult(
                workingDays,
                availableAnnual,
                neededFromNext,
                settings.FromNextBalanceMaxDays,
                balance.FromNextBalanceDaysUsed,
                pendingFromNext,
                false,
                $"Using next balance is only allowed between {settings.FromNextBalanceStartDate} and {settings.FromNextBalanceEndDate}."));
        }

        if (!balance.HasAvailableFromNextBalance(planner.Snapshot, neededFromNext, pendingFromNext))
        {
            return Result.Ok(new PreviewLeaveResult(
                workingDays,
                availableAnnual,
                neededFromNext,
                settings.FromNextBalanceMaxDays,
                balance.FromNextBalanceDaysUsed,
                pendingFromNext,
                false,
                $"You can use at most {settings.FromNextBalanceMaxDays} days from next balance."));
        }

        return Result.Ok(new PreviewLeaveResult(
            workingDays,
            availableAnnual,
            neededFromNext,
            settings.FromNextBalanceMaxDays,
            balance.FromNextBalanceDaysUsed,
            pendingFromNext,
            true,
            null));
    }
}
