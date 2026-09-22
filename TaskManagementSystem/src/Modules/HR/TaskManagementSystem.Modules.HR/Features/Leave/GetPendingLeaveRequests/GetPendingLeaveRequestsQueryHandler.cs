using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GetPendingLeaveRequests;

public sealed class GetPendingLeaveRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetPendingLeaveRequestsQuery, Result<IReadOnlyList<LeaveRequestResult>>>
{
    public async Task<Result<IReadOnlyList<LeaveRequestResult>>> Handle(
        GetPendingLeaveRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var searchResult = await unitOfWork.LeaveRequests.SearchAsync(
            new LeaveRequestSearchCriteria(
                request.ViewerUserId,
                request.ViewerRole,
                request.ViewerTeamId,
                Page: 1,
                PageSize: 100,
                Search: null,
                FromDate: null,
                ToDate: null,
                Status: LeaveStatus.Pending,
                Type: null,
                MyStatus: null),
            cancellationToken);

        var results = searchResult.Items.Select(lr => LeaveRequestResult.From(lr)).ToList();
        return Result.Ok<IReadOnlyList<LeaveRequestResult>>(results);
    }
}

