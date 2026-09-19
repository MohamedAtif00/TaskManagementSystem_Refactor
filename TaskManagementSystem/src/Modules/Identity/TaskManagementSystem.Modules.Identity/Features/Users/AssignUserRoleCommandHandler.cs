using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Users;

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
