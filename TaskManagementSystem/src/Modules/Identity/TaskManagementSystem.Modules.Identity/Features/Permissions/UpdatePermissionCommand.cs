using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;

namespace TaskManagementSystem.Modules.Identity.Features.Permissions;

public sealed record UpdatePermissionCommand(int PermissionId, string Name, string? Description)
    : ICommand<Result<PermissionDto>>;

public sealed class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpdatePermissionCommandHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePermissionCommand, Result<PermissionDto>>
{
    public async Task<Result<PermissionDto>> Handle(
        UpdatePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var permission = await unitOfWork.Permissions.GetByIdTrackedAsync(request.PermissionId, cancellationToken);
        if (permission is null)
        {
            return Result.Fail<PermissionDto>(IdentityErrors.PermissionNotFound);
        }

        var updateResult = permission.Update(request.Name, request.Description);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<PermissionDto>(IdentityResultMapper.ToApplicationError(updateResult.Error));
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(PermissionDto.From(permission));
    }
}
