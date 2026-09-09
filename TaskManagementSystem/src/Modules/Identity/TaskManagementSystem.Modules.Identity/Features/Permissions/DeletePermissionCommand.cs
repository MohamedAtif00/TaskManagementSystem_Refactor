using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Permissions;

public sealed record DeletePermissionCommand(int PermissionId) : ICommand<Result<NoValue>>;

public sealed class DeletePermissionCommandValidator : AbstractValidator<DeletePermissionCommand>
{
    public DeletePermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId).GreaterThan(0);
    }
}

public sealed class DeletePermissionCommandHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<DeletePermissionCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        DeletePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var permission = await unitOfWork.Permissions.GetByIdTrackedAsync(request.PermissionId, cancellationToken);
        if (permission is null)
        {
            return Result.Fail<NoValue>(IdentityErrors.PermissionNotFound);
        }

        var deleteResult = permission.CanDelete();
        if (!deleteResult.IsSuccess)
        {
            return Result.Fail<NoValue>(IdentityResultMapper.ToApplicationError(deleteResult.Error));
        }

        unitOfWork.Permissions.Remove(permission);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
