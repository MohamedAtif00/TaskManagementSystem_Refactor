using Microsoft.Extensions.Options;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public sealed class LeaveOpinionProcessor(
    IHrUnitOfWork unitOfWork,
    IMedicalCertificateStorage medicalCertificateStorage,
    IOptions<LeaveSettingsOptions> leaveSettings)
{
    private static readonly HashSet<string> OpinionRoles =
    [
        "Owner",
        "TeamLeader",
        "ProjectManger",
        "SectionHead"
    ];

    public async Task<Result<LeaveRequest>> ProcessAsync(
        int actorUserId,
        string actorRole,
        int leaveRequestId,
        bool isApproved,
        string? comment,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (!OpinionRoles.Contains(actorRole))
        {
            return Result.Fail<LeaveRequest>(HrErrors.LeaveOpinionNotAuthorized);
        }

        var leaveRequest = await unitOfWork.LeaveRequests.GetByIdTrackedAsync(leaveRequestId, cancellationToken);
        if (leaveRequest is null)
        {
            return Result.Fail<LeaveRequest>(HrErrors.LeaveRequestNotFound);
        }

        if (leaveRequest.Status == LeaveStatus.Cancelled)
        {
            return Result.Fail<LeaveRequest>(HrErrors.LeaveCannotCancel);
        }

        if (await unitOfWork.Opinions.ExistsForUserAsync(leaveRequestId, actorUserId, cancellationToken))
        {
            return Result.Fail<LeaveRequest>(HrErrors.LeaveOpinionAlreadyGiven);
        }

        var opinionResult = Opinion.Create(leaveRequestId, actorUserId, isApproved, comment, utcNow);
        if (!opinionResult.IsSuccess)
        {
            return Result.Fail<LeaveRequest>(HrResultMapper.ToApplicationError(opinionResult.Error));
        }

        if (actorRole == "Owner")
        {
            if (isApproved)
            {
                var balance = await unitOfWork.EmployeeBalances.GetByUserIdAsync(leaveRequest.UserId, cancellationToken);
                if (balance is null)
                {
                    return Result.Fail<LeaveRequest>(HrErrors.UserNotFound);
                }

                if (!await HasSufficientBalanceForApprovalAsync(leaveRequest, balance, cancellationToken))
                {
                    return Result.Fail<LeaveRequest>(HrErrors.InsufficientBalance);
                }

                var approveResult = leaveRequest.Approve();
                if (!approveResult.IsSuccess)
                {
                    return Result.Fail<LeaveRequest>(HrResultMapper.ToApplicationError(approveResult.Error));
                }

                await unitOfWork.EmployeeBalances.DeductLeaveAsync(leaveRequest, cancellationToken);

                if (leaveRequest.Type == LeaveType.Sick)
                {
                    await medicalCertificateStorage.DeleteAsync(leaveRequest.MedicalCertificatePath, cancellationToken);
                    leaveRequest.ClearMedicalCertificate();
                }
            }
            else
            {
                var rejectResult = leaveRequest.Reject();
                if (!rejectResult.IsSuccess)
                {
                    return Result.Fail<LeaveRequest>(HrResultMapper.ToApplicationError(rejectResult.Error));
                }
            }
        }

        await unitOfWork.Opinions.AddAsync(opinionResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(leaveRequest);
    }

    private async Task<bool> HasSufficientBalanceForApprovalAsync(
        LeaveRequest leaveRequest,
        EmployeeBalance balance,
        CancellationToken cancellationToken)
    {
        return leaveRequest.Type switch
        {
            LeaveType.Annual => balance.HasAvailableAnnualLeave(
                leaveRequest.WorkingDays,
                await unitOfWork.LeaveRequests.SumPendingWorkingDaysAsync(
                    leaveRequest.UserId,
                    LeaveType.Annual,
                    leaveRequest.Id,
                    cancellationToken)),
            LeaveType.Emergency => balance.HasAvailableEmergencyLeave(
                leaveRequest.WorkingDays,
                await unitOfWork.LeaveRequests.SumPendingWorkingDaysAsync(
                    leaveRequest.UserId,
                    LeaveType.Emergency,
                    leaveRequest.Id,
                    cancellationToken)),
            LeaveType.FromNextBalance => balance.HasAvailableFromNextBalance(
                new LeaveSettingsSnapshot(
                    leaveSettings.Value.FromNextBalanceMaxDays,
                    leaveSettings.Value.FromNextBalanceStartDate,
                    leaveSettings.Value.FromNextBalanceEndDate,
                    leaveSettings.Value.EmergencyBlackoutCutoffDate,
                    leaveSettings.Value.ResetDate),
                leaveRequest.WorkingDays,
                await unitOfWork.LeaveRequests.SumPendingWorkingDaysAsync(
                    leaveRequest.UserId,
                    LeaveType.FromNextBalance,
                    leaveRequest.Id,
                    cancellationToken)),
            LeaveType.Sick or LeaveType.UnpaidLeave => true,
            _ => false
        };
    }
}
