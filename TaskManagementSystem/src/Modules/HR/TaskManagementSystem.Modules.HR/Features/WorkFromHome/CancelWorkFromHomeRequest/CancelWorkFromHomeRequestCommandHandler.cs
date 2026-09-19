using MediatR;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.CancelWorkFromHomeRequest;

public sealed class CancelWorkFromHomeRequestCommandHandler(
    IHrUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CancelWorkFromHomeRequestCommand, Result<WorkFromHomeRequestResult>>
{
    public async Task<Result<WorkFromHomeRequestResult>> Handle(
        CancelWorkFromHomeRequestCommand request,
        CancellationToken cancellationToken)
    {
        var workFromHome = await unitOfWork.WorkFromHomeRequests.GetByIdTrackedAsync(
            request.WorkFromHomeRequestId,
            cancellationToken);

        if (workFromHome is null || workFromHome.UserId != request.UserId)
        {
            return Result.Fail<WorkFromHomeRequestResult>(HrErrors.WorkFromHomeRequestNotFound);
        }

        var wasApproved = workFromHome.Status == WorkFromHomeStatus.Approved;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var cancelResult = workFromHome.Cancel(utcNow);
        if (!cancelResult.IsSuccess)
        {
            return Result.Fail<WorkFromHomeRequestResult>(HrResultMapper.ToApplicationError(cancelResult.Error));
        }

        if (wasApproved)
        {
            await unitOfWork.EmployeeBalances.RefundWorkFromHomeAsync(workFromHome.UserId, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(WorkFromHomeRequestResult.From(workFromHome));
    }
}

