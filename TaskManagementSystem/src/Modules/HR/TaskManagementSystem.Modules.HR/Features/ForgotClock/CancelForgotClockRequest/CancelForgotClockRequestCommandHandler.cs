using MediatR;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.CancelForgotClockRequest;

public sealed class CancelForgotClockRequestCommandHandler(
    IHrUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CancelForgotClockRequestCommand, Result<ForgotClockRequestResult>>
{
    public async Task<Result<ForgotClockRequestResult>> Handle(
        CancelForgotClockRequestCommand request,
        CancellationToken cancellationToken)
    {
        var forgotClock = await unitOfWork.ForgotClockRequests.GetByIdTrackedAsync(
            request.ForgotClockRequestId,
            cancellationToken);

        if (forgotClock is null || forgotClock.UserId != request.UserId)
        {
            return Result.Fail<ForgotClockRequestResult>(HrErrors.ForgotClockRequestNotFound);
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var cancelResult = forgotClock.Cancel(utcNow);
        if (!cancelResult.IsSuccess)
        {
            return Result.Fail<ForgotClockRequestResult>(HrResultMapper.ToApplicationError(cancelResult.Error));
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(ForgotClockRequestResult.From(forgotClock));
    }
}

