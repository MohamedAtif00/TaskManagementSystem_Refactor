using MediatR;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.GetPendingPermissionRequests;

public sealed class GetPendingPermissionRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetPendingPermissionRequestsQuery, Result<IReadOnlyList<PermissionRequestResult>>>
{
    public async Task<Result<IReadOnlyList<PermissionRequestResult>>> Handle(
        GetPendingPermissionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await unitOfWork.PermissionRequests.GetPendingAsync(cancellationToken);
        var results = permissions.Select(p => PermissionRequestResult.From(p)).ToList();
        return Result.Ok<IReadOnlyList<PermissionRequestResult>>(results);
    }
}

