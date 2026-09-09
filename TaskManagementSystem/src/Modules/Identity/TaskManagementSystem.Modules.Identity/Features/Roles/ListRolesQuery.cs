using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed record ListRolesQuery : IQuery<Result<IReadOnlyList<RoleDto>>>;

public sealed class ListRolesQueryHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<ListRolesQuery, Result<IReadOnlyList<RoleDto>>>
{
    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(
        ListRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await unitOfWork.Roles.ListWithPermissionsAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<RoleDto>>(roles.Select(RoleDto.From).ToList());
    }
}
