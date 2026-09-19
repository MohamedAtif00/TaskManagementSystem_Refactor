using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed class SetRolePermissionsCommandHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<SetRolePermissionsCommand, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(
        SetRolePermissionsCommand request,
        CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Roles.GetByIdTrackedWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Fail<RoleDto>(IdentityErrors.RoleNotFound);
        }

        var permissions = await unitOfWork.Permissions.GetByIdsAsync(request.PermissionIds, cancellationToken);
        if (permissions.Count != request.PermissionIds.Distinct().Count())
        {
            return Result.Fail<RoleDto>(IdentityErrors.PermissionNotFound);
        }

        role.ReplacePermissions(permissions);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(RoleDto.From(role));
    }
}
