using MediatR;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.SearchWorkFromHomeRequests;

public sealed class SearchWorkFromHomeRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<SearchWorkFromHomeRequestsQuery, Result<WorkFromHomeRequestListResult>>
{
    public async Task<Result<WorkFromHomeRequestListResult>> Handle(
        SearchWorkFromHomeRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var searchResult = await unitOfWork.WorkFromHomeRequests.SearchAsync(
            new WorkFromHomeRequestSearchCriteria(
                request.ViewerUserId,
                request.ViewerRole,
                request.ViewerTeamId,
                request.Page,
                request.PageSize,
                request.Search,
                request.FromDate,
                request.ToDate,
                request.Status,
                request.MyStatus),
            cancellationToken);

        var items = searchResult.Items.Select(r => WorkFromHomeRequestResult.From(r)).ToList();
        return Result.Ok(new WorkFromHomeRequestListResult(
            items,
            request.Page,
            request.PageSize,
            searchResult.TotalCount));
    }
}

