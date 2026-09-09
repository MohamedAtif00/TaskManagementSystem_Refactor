using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Modules.Identity.Features.Roles;

public sealed record CreateRoleCommand(
    string Name,
    string? Description,
    IReadOnlyList<int> PermissionIds) : ICommand<Result<RoleDto>>;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class CreateRoleCommandHandler(IIdentityUnitOfWork unitOfWork)
    : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var createResult = Role.Create(request.Name, request.Description);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<RoleDto>(IdentityResultMapper.ToApplicationError(createResult.Error));
        }

        var existing = await unitOfWork.Roles.GetByNameAsync(createResult.Value.Name, cancellationToken);
        if (existing is not null)
        {
            return Result.Fail<RoleDto>(IdentityErrors.DuplicateRoleName);
        }

        var role = createResult.Value;
        role.Id = await unitOfWork.Roles.GetNextIdAsync(cancellationToken);

        var permissions = await unitOfWork.Permissions.GetByIdsAsync(request.PermissionIds, cancellationToken);
        if (permissions.Count != request.PermissionIds.Distinct().Count())
        {
            return Result.Fail<RoleDto>(IdentityErrors.PermissionNotFound);
        }

        role.ReplacePermissions(permissions);
        await unitOfWork.Roles.AddAsync(role, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(RoleDto.From(role));
    }
}
