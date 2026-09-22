using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public sealed class PermissionOpinionProcessor(IHrUnitOfWork unitOfWork)
{
    public async Task<Result<PermissionRequest>> ProcessAsync(
        int actorUserId,
        string actorRole,
        int permissionId,
        bool isApproved,
        string? comment,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var permission = await unitOfWork.PermissionRequests.GetByIdTrackedAsync(permissionId, cancellationToken);
        if (permission is null)
        {
            return Result.Fail<PermissionRequest>(HrErrors.PermissionRequestNotFound);
        }

        if (!HrOpinionAuthorization.CanGiveOpinion(
                actorUserId,
                actorRole,
                permission.UserId,
                permission.TeamleaderId,
                permission.SectionheadId))
        {
            return Result.Fail<PermissionRequest>(HrErrors.PermissionOpinionNotAuthorized);
        }

        if (permission.Status == PermissionStatus.Cancelled)
        {
            return Result.Fail<PermissionRequest>(HrErrors.PermissionCannotCancel);
        }

        if (await unitOfWork.Opinions.ExistsForPermissionUserAsync(permissionId, actorUserId, cancellationToken))
        {
            return Result.Fail<PermissionRequest>(HrErrors.PermissionOpinionAlreadyGiven);
        }

        var opinionResult = Opinion.CreateForPermission(permissionId, actorUserId, isApproved, comment, utcNow);
        if (!opinionResult.IsSuccess)
        {
            return Result.Fail<PermissionRequest>(HrResultMapper.ToApplicationError(opinionResult.Error));
        }

        if (actorRole == "Owner")
        {
            if (isApproved)
            {
                var balance = await unitOfWork.EmployeeBalances.GetByUserIdAsync(permission.UserId, cancellationToken);
                if (balance is null)
                {
                    return Result.Fail<PermissionRequest>(HrErrors.UserNotFound);
                }

                var pendingCount = await unitOfWork.PermissionRequests.CountPendingAsync(
                    permission.UserId,
                    permission.Id,
                    cancellationToken);

                if (!balance.HasAvailablePermission(pendingCount))
                {
                    return Result.Fail<PermissionRequest>(HrErrors.PermissionInsufficientBalance);
                }

                var deductResult = await unitOfWork.EmployeeBalances.DeductPermissionAsync(
                    permission.UserId,
                    cancellationToken);
                if (!deductResult.IsSuccess)
                {
                    return Result.Fail<PermissionRequest>(deductResult.Error);
                }

                var approveResult = permission.Approve(utcNow);
                if (!approveResult.IsSuccess)
                {
                    return Result.Fail<PermissionRequest>(HrResultMapper.ToApplicationError(approveResult.Error));
                }
            }
            else
            {
                var rejectResult = permission.Reject(utcNow);
                if (!rejectResult.IsSuccess)
                {
                    return Result.Fail<PermissionRequest>(HrResultMapper.ToApplicationError(rejectResult.Error));
                }
            }
        }

        await unitOfWork.Opinions.AddAsync(opinionResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(permission);
    }
}
