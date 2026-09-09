using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed record DeleteRoleCommand(int RoleId) : ICommand<Result<NoValue>>;

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
    }
}

public sealed class DeleteRoleCommandHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<DeleteRoleCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        DeleteRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await unitOfWork.Roles.GetByIdTrackedWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Fail<NoValue>(IdentityErrors.RoleNotFound);
        }

        var hasAssignedUsers = await unitOfWork.Roles.HasAssignedUsersAsync(role.Id, cancellationToken);
        var deleteResult = role.CanDelete(hasAssignedUsers);
        if (!deleteResult.IsSuccess)
        {
            return Result.Fail<NoValue>(IdentityResultMapper.ToApplicationError(deleteResult.Error));
        }

        unitOfWork.Roles.Remove(role);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
