using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.CancelLeaveRequest;

public sealed record CancelLeaveRequestCommand(int UserId, int LeaveRequestId)
    : ICommand<Result<LeaveRequestResult>>;

public sealed class CancelLeaveRequestCommandHandler(
    IHrUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CancelLeaveRequestCommand, Result<LeaveRequestResult>>
{
    public async Task<Result<LeaveRequestResult>> Handle(
        CancelLeaveRequestCommand request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await unitOfWork.LeaveRequests.GetByIdTrackedAsync(
            request.LeaveRequestId,
            cancellationToken);

        if (leaveRequest is null || leaveRequest.UserId != request.UserId)
        {
            return Result.Fail<LeaveRequestResult>(HrErrors.LeaveRequestNotFound);
        }

        var wasApproved = leaveRequest.Status == LeaveStatus.Approved;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var cancelResult = leaveRequest.Cancel(utcNow);
        if (!cancelResult.IsSuccess)
        {
            return Result.Fail<LeaveRequestResult>(HrResultMapper.ToApplicationError(cancelResult.Error));
        }

        if (wasApproved)
        {
            await unitOfWork.EmployeeBalances.RefundLeaveAsync(leaveRequest, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(LeaveRequestResult.From(leaveRequest));
    }
}
