using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GetPendingLeaveRequests;

public sealed record GetPendingLeaveRequestsQuery : IQuery<Result<IReadOnlyList<LeaveRequestResult>>>;

public sealed class GetPendingLeaveRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetPendingLeaveRequestsQuery, Result<IReadOnlyList<LeaveRequestResult>>>
{
    public async Task<Result<IReadOnlyList<LeaveRequestResult>>> Handle(
        GetPendingLeaveRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var leaveRequests = await unitOfWork.LeaveRequests.GetPendingAsync(cancellationToken);
        var results = leaveRequests.Select(lr => LeaveRequestResult.From(lr)).ToList();
        return Result.Ok<IReadOnlyList<LeaveRequestResult>>(results);
    }
}
