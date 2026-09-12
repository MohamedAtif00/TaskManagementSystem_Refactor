using Microsoft.Extensions.Options;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Infrastructure;

public sealed class LeaveRequestPlanner(
    IOptions<LeaveSettingsOptions> leaveSettings,
    IWorkingDayCalculator workingDayCalculator,
    OrgLookupQueries orgLookupQueries)
{
    public LeaveSettingsOptions Settings => leaveSettings.Value;

    public LeaveSettingsSnapshot Snapshot =>
        new(
            Settings.FromNextBalanceMaxDays,
            Settings.FromNextBalanceStartDate,
            Settings.FromNextBalanceEndDate,
            Settings.EmergencyBlackoutCutoffDate,
            Settings.ResetDate);

    public async Task<Result<IReadOnlyList<LeaveRequest>>> PlanAsync(
        IHrUnitOfWork unitOfWork,
        LeaveRequestPlan request,
        CancellationToken cancellationToken)
    {
        var balance = await unitOfWork.EmployeeBalances.GetByUserIdAsync(request.UserId, cancellationToken);
        if (balance is null)
        {
            return Result.Fail<IReadOnlyList<LeaveRequest>>(HrErrors.UserNotFound);
        }

        int? sectionHeadId = null;
        if (request.RequesterRole == "TeamLeader" && balance.TeamId.HasValue)
        {
            sectionHeadId = await orgLookupQueries.GetSectionHeadIdForTeamAsync(balance.TeamId.Value, cancellationToken);
        }

        var validation = await ValidateTypeRulesAsync(unitOfWork, balance, request, cancellationToken);
        if (!validation.IsSuccess)
        {
            return Result.Fail<IReadOnlyList<LeaveRequest>>(validation.Error);
        }

        var segments = await BuildSegmentsAsync(unitOfWork, balance, request, cancellationToken);
        var leaveRequests = new List<LeaveRequest>();

        foreach (var segment in segments)
        {
            var createResult = await CreateSegmentAsync(segment, request, balance.TeamleaderId, sectionHeadId, cancellationToken);
            if (!createResult.IsSuccess)
            {
                return Result.Fail<IReadOnlyList<LeaveRequest>>(createResult.Error);
            }

            leaveRequests.Add(createResult.Value);
        }

        return Result.Ok<IReadOnlyList<LeaveRequest>>(leaveRequests);
    }

    private async Task<Result<NoValue>> ValidateTypeRulesAsync(
        IHrUnitOfWork unitOfWork,
        EmployeeBalance balance,
        LeaveRequestPlan request,
        CancellationToken cancellationToken)
    {
        var workingDays = await workingDayCalculator.CountAsync(request.StartDate, request.EndDate, cancellationToken);
        if (workingDays == 0)
        {
            return HrErrors.InvalidDates;
        }

        var today = request.UtcNow.Date;

        if (request.Type == LeaveType.Emergency && !LeaveSettingsHelper.IsEmergencyAllowed(Settings, today))
        {
            return HrErrors.LeaveEmergencyBlackout;
        }

        if ((request.Type == LeaveType.FromNextBalance || request.ConfirmFromNextBalance) &&
            !LeaveSettingsHelper.IsFromNextAvailable(Settings, today))
        {
            return HrErrors.LeaveFromNextUnavailable;
        }

        if (request.Type == LeaveType.FromNextBalance)
        {
            if (!LeaveSettingsHelper.IsInFromNextWindow(Settings, today))
            {
                return HrErrors.LeaveFromNextUnavailable;
            }

            var pendingFromNext = await unitOfWork.LeaveRequests.SumPendingWorkingDaysAsync(
                request.UserId,
                LeaveType.FromNextBalance,
                cancellationToken: cancellationToken);

            if (!balance.HasAvailableFromNextBalance(Snapshot, workingDays, pendingFromNext))
            {
                return HrErrors.InsufficientBalance;
            }

            return Result.Ok();
        }

        if (request.Type == LeaveType.Emergency)
        {
            var pendingEmergency = await unitOfWork.LeaveRequests.SumPendingWorkingDaysAsync(
                request.UserId,
                LeaveType.Emergency,
                cancellationToken: cancellationToken);

            if (!balance.HasAvailableEmergencyLeave(workingDays, pendingEmergency))
            {
                return HrErrors.InsufficientBalance;
            }

            return Result.Ok();
        }

        if (request.Type == LeaveType.Annual)
        {
            var pendingAnnual = await unitOfWork.LeaveRequests.SumPendingWorkingDaysAsync(
                request.UserId,
                LeaveType.Annual,
                cancellationToken: cancellationToken);

            var remainingAnnual = balance.AvailableAnnualLeave(pendingAnnual);
            if (workingDays <= remainingAnnual)
            {
                return Result.Ok();
            }

            var neededFromNext = workingDays - remainingAnnual;
            if (!LeaveSettingsHelper.IsInFromNextWindow(Settings, today))
            {
                return HrErrors.InsufficientBalance;
            }

            var pendingFromNext = await unitOfWork.LeaveRequests.SumPendingWorkingDaysAsync(
                request.UserId,
                LeaveType.FromNextBalance,
                cancellationToken: cancellationToken);

            if (!balance.HasAvailableFromNextBalance(Snapshot, neededFromNext, pendingFromNext))
            {
                return HrErrors.InsufficientBalance;
            }

            if (!request.ConfirmFromNextBalance)
            {
                return HrErrors.LeaveFromNextConfirmationRequired;
            }

            return Result.Ok();
        }

        if (request.Type == LeaveType.Sick && workingDays > 3 && !request.HasMedicalCertificate)
        {
            return HrErrors.LeaveMedicalRequired;
        }

        return Result.Ok();
    }

    private async Task<IReadOnlyList<LeaveSegment>> BuildSegmentsAsync(
        IHrUnitOfWork unitOfWork,
        EmployeeBalance balance,
        LeaveRequestPlan request,
        CancellationToken cancellationToken)
    {
        if (request.Type != LeaveType.Annual || !request.ConfirmFromNextBalance)
        {
            return [new LeaveSegment(request.StartDate, request.EndDate, request.Type)];
        }

        var workingDaysInRange = await workingDayCalculator.GetWorkingDaysInRangeAsync(
            request.StartDate,
            request.EndDate,
            cancellationToken);
        var totalDays = workingDaysInRange.Count;
        var pendingAnnual = await unitOfWork.LeaveRequests.SumPendingWorkingDaysAsync(
            request.UserId,
            LeaveType.Annual,
            cancellationToken: cancellationToken);
        var remainingAnnual = balance.AvailableAnnualLeave(pendingAnnual);
        var neededFromNext = Math.Max(0, totalDays - remainingAnnual);

        if (neededFromNext == 0)
        {
            return [new LeaveSegment(request.StartDate, request.EndDate, LeaveType.Annual)];
        }

        var annualDays = totalDays - neededFromNext;
        if (annualDays <= 0)
        {
            return [new LeaveSegment(request.StartDate, request.EndDate, LeaveType.FromNextBalance)];
        }

        var (start1, end1, start2, end2) = LeaveDateRangeSplitter.SplitByWorkingDays(
            workingDaysInRange,
            annualDays);

        var segments = new List<LeaveSegment>();
        if (start1.HasValue && end1.HasValue)
        {
            segments.Add(new LeaveSegment(start1.Value, end1.Value, LeaveType.Annual));
        }

        if (start2.HasValue && end2.HasValue)
        {
            segments.Add(new LeaveSegment(start2.Value, end2.Value, LeaveType.FromNextBalance));
        }

        return segments;
    }

    private async Task<Result<LeaveRequest>> CreateSegmentAsync(
        LeaveSegment segment,
        LeaveRequestPlan request,
        int? teamleaderId,
        int? sectionHeadId,
        CancellationToken cancellationToken)
    {
        var segmentWorkingDays = await workingDayCalculator.CountAsync(
            segment.StartDate,
            segment.EndDate,
            cancellationToken);

        return segment.Type switch
        {
            LeaveType.Annual => LeaveRequest.CreateAnnual(
                request.UserId, segment.StartDate, segment.EndDate, segmentWorkingDays, request.Reason, request.NoteForManager,
                teamleaderId, sectionHeadId, request.UtcNow),
            LeaveType.Emergency => LeaveRequest.CreateEmergency(
                request.UserId, segment.StartDate, segment.EndDate, segmentWorkingDays, request.Reason, request.NoteForManager,
                teamleaderId, sectionHeadId, request.UtcNow),
            LeaveType.Sick => LeaveRequest.CreateSick(
                request.UserId, segment.StartDate, segment.EndDate, segmentWorkingDays, request.Reason, request.NoteForManager,
                teamleaderId, sectionHeadId, request.UtcNow, request.HasMedicalCertificate),
            LeaveType.UnpaidLeave => LeaveRequest.CreateUnpaid(
                request.UserId, segment.StartDate, segment.EndDate, segmentWorkingDays, request.Reason, request.NoteForManager,
                teamleaderId, sectionHeadId, request.UtcNow),
            LeaveType.FromNextBalance => LeaveRequest.CreateFromNextBalance(
                request.UserId, segment.StartDate, segment.EndDate, segmentWorkingDays, request.Reason, request.NoteForManager,
                teamleaderId, sectionHeadId, request.UtcNow),
            _ => Result.Fail<LeaveRequest>(HrErrors.LeaveTypeNotSupported)
        };
    }

    private sealed record LeaveSegment(DateTime StartDate, DateTime EndDate, LeaveType Type);
}

public sealed record LeaveRequestPlan(
    int UserId,
    string RequesterRole,
    LeaveType Type,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason,
    string? NoteForManager,
    bool ConfirmFromNextBalance,
    bool HasMedicalCertificate,
    DateTime UtcNow);
