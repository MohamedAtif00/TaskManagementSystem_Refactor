using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Users;

public sealed record AssignUserRoleCommand(int UserId, int RoleId) : ICommand<Result<NoValue>>;

public sealed class AssignUserRoleCommandValidator : AbstractValidator<AssignUserRoleCommand>
{
    public AssignUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
    }
}

public sealed class AssignUserRoleCommandHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<AssignUserRoleCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        AssignUserRoleCommand request,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdTrackedAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Fail<NoValue>(IdentityErrors.UserNotFound);
        }

        var role = await unitOfWork.Roles.GetByIdWithPermissionsAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Fail<NoValue>(IdentityErrors.RoleNotFound);
        }

        user.AssignRole(role.Id);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
