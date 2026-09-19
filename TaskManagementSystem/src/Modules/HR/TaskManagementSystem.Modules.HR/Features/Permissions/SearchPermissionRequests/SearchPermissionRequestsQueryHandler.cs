using MediatR;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.SearchPermissionRequests;

public sealed class SearchPermissionRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<SearchPermissionRequestsQuery, Result<PermissionRequestListResult>>
{
    public async Task<Result<PermissionRequestListResult>> Handle(
        SearchPermissionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var searchResult = await unitOfWork.PermissionRequests.SearchAsync(
            new PermissionRequestSearchCriteria(
                request.ViewerUserId,
                request.ViewerRole,
                request.ViewerTeamId,
                request.Page,
                request.PageSize,
                request.Search,
                request.Date,
                request.FromDate,
                request.ToDate,
                request.Status,
                request.Type,
                request.MyStatus),
            cancellationToken);

        var items = searchResult.Items.Select(p => PermissionRequestResult.From(p)).ToList();
        return Result.Ok(new PermissionRequestListResult(items, searchResult.TotalCount));
    }
}

