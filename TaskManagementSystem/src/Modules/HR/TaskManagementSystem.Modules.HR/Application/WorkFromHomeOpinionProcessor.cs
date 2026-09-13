using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public sealed class WorkFromHomeOpinionProcessor(IHrUnitOfWork unitOfWork)
{
    private static readonly HashSet<string> OpinionRoles =
    [
        "Owner",
        "TeamLeader",
        "ProjectManger",
        "SectionHead"
    ];

    public async Task<Result<WorkFromHomeRequest>> ProcessAsync(
        int actorUserId,
        string actorRole,
        int workFromHomeRequestId,
        bool isApproved,
        string? comment,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (!OpinionRoles.Contains(actorRole))
        {
            return Result.Fail<WorkFromHomeRequest>(HrErrors.WorkFromHomeOpinionNotAuthorized);
        }

        var request = await unitOfWork.WorkFromHomeRequests.GetByIdTrackedAsync(workFromHomeRequestId, cancellationToken);
        if (request is null)
        {
            return Result.Fail<WorkFromHomeRequest>(HrErrors.WorkFromHomeRequestNotFound);
        }

        if (request.Status == WorkFromHomeStatus.Cancelled)
        {
            return Result.Fail<WorkFromHomeRequest>(HrErrors.WorkFromHomeCannotCancel);
        }

        if (await unitOfWork.Opinions.ExistsForWorkFromHomeUserAsync(workFromHomeRequestId, actorUserId, cancellationToken))
        {
            return Result.Fail<WorkFromHomeRequest>(HrErrors.WorkFromHomeOpinionAlreadyGiven);
        }

        var opinionResult = Opinion.CreateForWorkFromHome(workFromHomeRequestId, actorUserId, isApproved, comment, utcNow);
        if (!opinionResult.IsSuccess)
        {
            return Result.Fail<WorkFromHomeRequest>(HrResultMapper.ToApplicationError(opinionResult.Error));
        }

        if (actorRole == "Owner")
        {
            if (isApproved)
            {
                var balance = await unitOfWork.EmployeeBalances.GetByUserIdAsync(request.UserId, cancellationToken);
                if (balance is null)
                {
                    return Result.Fail<WorkFromHomeRequest>(HrErrors.UserNotFound);
                }

                var pendingCount = await unitOfWork.WorkFromHomeRequests.CountPendingAsync(
                    request.UserId,
                    request.Id,
                    cancellationToken);

                if (!balance.HasAvailableWorkFromHome(pendingCount))
                {
                    return Result.Fail<WorkFromHomeRequest>(HrErrors.WorkFromHomeInsufficientBalance);
                }

                var approveResult = request.Approve();
                if (!approveResult.IsSuccess)
                {
                    return Result.Fail<WorkFromHomeRequest>(HrResultMapper.ToApplicationError(approveResult.Error));
                }

                await unitOfWork.EmployeeBalances.DeductWorkFromHomeAsync(request.UserId, cancellationToken);
            }
            else
            {
                var rejectResult = request.Reject();
                if (!rejectResult.IsSuccess)
                {
                    return Result.Fail<WorkFromHomeRequest>(HrResultMapper.ToApplicationError(rejectResult.Error));
                }
            }
        }

        await unitOfWork.Opinions.AddAsync(opinionResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(request);
    }
}
