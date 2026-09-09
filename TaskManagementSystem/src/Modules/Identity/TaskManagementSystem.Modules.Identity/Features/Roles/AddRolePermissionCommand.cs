using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed record AddRolePermissionCommand(int RoleId, int PermissionId) : ICommand<Result<RoleDto>>;

public sealed class AddRolePermissionCommandValidator : AbstractValidator<AddRolePermissionCommand>
{
    public AddRolePermissionCommandValidator()
    {
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PermissionId).GreaterThan(0);
    }
}

public sealed class AddRolePermissionCommandHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<AddRolePermissionCommand, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(
        AddRolePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Roles.GetByIdTrackedWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Fail<RoleDto>(IdentityErrors.RoleNotFound);
        }

        var permissions = await unitOfWork.Permissions.GetByIdsAsync([request.PermissionId], cancellationToken);
        var permission = permissions.FirstOrDefault();
        if (permission is null)
        {
            return Result.Fail<RoleDto>(IdentityErrors.PermissionNotFound);
        }

        if (role.Permissions.All(existing => existing.Id != permission.Id))
        {
            role.AddPermission(permission);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return Result.Ok(RoleDto.From(role));
    }
}
