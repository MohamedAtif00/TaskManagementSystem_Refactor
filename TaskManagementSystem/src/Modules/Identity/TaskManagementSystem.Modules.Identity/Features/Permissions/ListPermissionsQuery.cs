using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Permissions;

public sealed record ListPermissionsQuery : IQuery<Result<IReadOnlyList<PermissionDto>>>;

public sealed class ListPermissionsQueryHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<ListPermissionsQuery, Result<IReadOnlyList<PermissionDto>>>
{
    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(
        ListPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await unitOfWork.Permissions.ListAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<PermissionDto>>(permissions.Select(PermissionDto.From).ToList());
    }
}
