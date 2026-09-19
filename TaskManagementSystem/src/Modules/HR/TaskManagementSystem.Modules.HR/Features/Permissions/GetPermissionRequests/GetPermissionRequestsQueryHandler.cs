using MediatR;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.GetPermissionRequests;

public sealed class GetPermissionRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetPermissionRequestsQuery, Result<IReadOnlyList<PermissionRequestResult>>>
{
    public async Task<Result<IReadOnlyList<PermissionRequestResult>>> Handle(
        GetPermissionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await unitOfWork.PermissionRequests.GetByUserIdAsync(request.UserId, cancellationToken);
        var results = permissions.Select(p => PermissionRequestResult.From(p)).ToList();
        return Result.Ok<IReadOnlyList<PermissionRequestResult>>(results);
    }
}

