using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Permissions;

public sealed record CreatePermissionCommand(string Code, string Name, string? Description)
    : ICommand<Result<PermissionDto>>;

public sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class CreatePermissionCommandHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<CreatePermissionCommand, Result<PermissionDto>>
{
    public async Task<Result<PermissionDto>> Handle(
        CreatePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var createResult = Permission.Create(request.Code, request.Name, request.Description);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<PermissionDto>(IdentityResultMapper.ToApplicationError(createResult.Error));
        }

        var existing = await unitOfWork.Permissions.GetByCodeAsync(createResult.Value.Code, cancellationToken);
        if (existing is not null)
        {
            return Result.Fail<PermissionDto>(IdentityErrors.DuplicatePermissionCode);
        }

        await unitOfWork.Permissions.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(PermissionDto.From(createResult.Value));
    }
}
