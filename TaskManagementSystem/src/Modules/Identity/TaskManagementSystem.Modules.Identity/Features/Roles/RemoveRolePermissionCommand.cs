using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed record RemoveRolePermissionCommand(int RoleId, int PermissionId) : ICommand<Result<RoleDto>>;

public sealed class RemoveRolePermissionCommandValidator : AbstractValidator<RemoveRolePermissionCommand>
{
    public RemoveRolePermissionCommandValidator()
    {
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PermissionId).GreaterThan(0);
    }
}

public sealed class RemoveRolePermissionCommandHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<RemoveRolePermissionCommand, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(
        RemoveRolePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Roles.GetByIdTrackedWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Fail<RoleDto>(IdentityErrors.RoleNotFound);
        }

        var permission = role.Permissions.FirstOrDefault(existing => existing.Id == request.PermissionId);
        if (permission is null)
        {
            return Result.Fail<RoleDto>(IdentityErrors.PermissionNotFound);
        }

        role.RemovePermission(permission);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(RoleDto.From(role));
    }
}
