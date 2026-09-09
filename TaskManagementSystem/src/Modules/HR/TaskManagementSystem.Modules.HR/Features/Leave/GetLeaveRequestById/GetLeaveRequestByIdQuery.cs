using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GetLeaveRequestById;

public sealed record GetLeaveRequestByIdQuery(int UserId, string UserRole, int LeaveRequestId)
    : IQuery<Result<LeaveRequestResult>>;

public sealed class GetLeaveRequestByIdQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetLeaveRequestByIdQuery, Result<LeaveRequestResult>>
{
    public async Task<Result<LeaveRequestResult>> Handle(
        GetLeaveRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await unitOfWork.LeaveRequests.GetByIdAsync(request.LeaveRequestId, cancellationToken);
        if (leaveRequest is null)
        {
            return Result.Fail<LeaveRequestResult>(HrErrors.LeaveRequestNotFound);
        }

        if (leaveRequest.UserId != request.UserId && request.UserRole != "Owner")
        {
            return Result.Fail<LeaveRequestResult>(HrErrors.LeaveRequestNotFound);
        }

        var opinions = await unitOfWork.Opinions.GetByLeaveRequestIdAsync(request.LeaveRequestId, cancellationToken);
        var opinionResults = opinions.Select(OpinionResult.From).ToList();

        return Result.Ok(LeaveRequestResult.From(leaveRequest, opinionResults));
    }
}
