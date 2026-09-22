using MediatR;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.GetPendingPermissionRequests;

public sealed class GetPendingPermissionRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetPendingPermissionRequestsQuery, Result<IReadOnlyList<PermissionRequestResult>>>
{
    public async Task<Result<IReadOnlyList<PermissionRequestResult>>> Handle(
        GetPendingPermissionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var searchResult = await unitOfWork.PermissionRequests.SearchAsync(
            new PermissionRequestSearchCriteria(
                request.ViewerUserId,
                request.ViewerRole,
                request.ViewerTeamId,
                Page: 1,
                PageSize: 100,
                Search: null,
                Date: null,
                FromDate: null,
                ToDate: null,
                Status: PermissionStatus.Pending,
                Type: null,
                MyStatus: null),
            cancellationToken);

        var results = searchResult.Items.Select(p => PermissionRequestResult.From(p)).ToList();
        return Result.Ok<IReadOnlyList<PermissionRequestResult>>(results);
    }
}

