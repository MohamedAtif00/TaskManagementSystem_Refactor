using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public sealed class ForgotClockOpinionProcessor(IHrUnitOfWork unitOfWork)
{
    private static readonly HashSet<string> OpinionRoles =
    [
        "Owner",
        "TeamLeader",
        "ProjectManger",
        "SectionHead"
    ];

    public async Task<Result<ForgotClockRequest>> ProcessAsync(
        int actorUserId,
        string actorRole,
        int forgotClockRequestId,
        bool isApproved,
        string? comment,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        if (!OpinionRoles.Contains(actorRole))
        {
            return Result.Fail<ForgotClockRequest>(HrErrors.ForgotClockOpinionNotAuthorized);
        }

        var request = await unitOfWork.ForgotClockRequests.GetByIdTrackedAsync(forgotClockRequestId, cancellationToken);
        if (request is null)
        {
            return Result.Fail<ForgotClockRequest>(HrErrors.ForgotClockRequestNotFound);
        }

        if (request.Status == ForgotClockStatus.Cancelled)
        {
            return Result.Fail<ForgotClockRequest>(HrErrors.ForgotClockCannotCancel);
        }

        if (await unitOfWork.Opinions.ExistsForForgotClockUserAsync(forgotClockRequestId, actorUserId, cancellationToken))
        {
            return Result.Fail<ForgotClockRequest>(HrErrors.ForgotClockOpinionAlreadyGiven);
        }

        var opinionResult = Opinion.CreateForForgotClock(forgotClockRequestId, actorUserId, isApproved, comment, utcNow);
        if (!opinionResult.IsSuccess)
        {
            return Result.Fail<ForgotClockRequest>(HrResultMapper.ToApplicationError(opinionResult.Error));
        }

        if (actorRole == "Owner")
        {
            if (isApproved)
            {
                var approveResult = request.Approve(utcNow);
                if (!approveResult.IsSuccess)
                {
                    return Result.Fail<ForgotClockRequest>(HrResultMapper.ToApplicationError(approveResult.Error));
                }
            }
            else
            {
                var rejectResult = request.Reject(utcNow);
                if (!rejectResult.IsSuccess)
                {
                    return Result.Fail<ForgotClockRequest>(HrResultMapper.ToApplicationError(rejectResult.Error));
                }
            }
        }

        await unitOfWork.Opinions.AddAsync(opinionResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(request);
    }
}
