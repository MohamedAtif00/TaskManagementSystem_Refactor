using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed class UpdateRoleCommandHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<UpdateRoleCommand, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Roles.GetByIdTrackedWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Fail<RoleDto>(IdentityErrors.RoleNotFound);
        }

        if (!role.IsSystem)
        {
            var existing = await unitOfWork.Roles.GetByNameAsync(request.Name.Trim(), cancellationToken);
            if (existing is not null && existing.Id != role.Id)
            {
                return Result.Fail<RoleDto>(IdentityErrors.DuplicateRoleName);
            }
        }

        var updateResult = role.Update(request.Name, request.Description);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<RoleDto>(IdentityResultMapper.ToApplicationError(updateResult.Error));
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(RoleDto.From(role));
    }
}
