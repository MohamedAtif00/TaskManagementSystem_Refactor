using MediatR;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GetPendingWorkFromHomeRequests;

public sealed class GetPendingWorkFromHomeRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetPendingWorkFromHomeRequestsQuery, Result<IReadOnlyList<WorkFromHomeRequestResult>>>
{
    public async Task<Result<IReadOnlyList<WorkFromHomeRequestResult>>> Handle(
        GetPendingWorkFromHomeRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var searchResult = await unitOfWork.WorkFromHomeRequests.SearchAsync(
            new WorkFromHomeRequestSearchCriteria(
                request.ViewerUserId,
                request.ViewerRole,
                request.ViewerTeamId,
                Page: 1,
                PageSize: 100,
                Search: null,
                FromDate: null,
                ToDate: null,
                Status: WorkFromHomeStatus.Pending,
                MyStatus: null),
            cancellationToken);

        var results = searchResult.Items.Select(r => WorkFromHomeRequestResult.From(r)).ToList();
        return Result.Ok<IReadOnlyList<WorkFromHomeRequestResult>>(results);
    }
}

