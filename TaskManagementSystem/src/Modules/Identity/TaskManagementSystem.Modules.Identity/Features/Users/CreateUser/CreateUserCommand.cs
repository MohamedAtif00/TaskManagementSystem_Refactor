using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Identity.Features.Users.CreateUser;

public sealed record CreateUserCommand(
    string Name,
    string HrCode,
    string? Email,
    string? Phone,
    string? Title,
    int RoleId,
    AccountType AccountType,
    int? TeamId,
    int? TeamleaderId) : ICommand<Result<UserDetailResult>>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.HrCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Title).MaximumLength(100);
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateUserCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    IUserAdminQueries userAdminQueries,
    OrganizationLookupQueries organizationLookupQueries,
    EmployeeBalanceCommands employeeBalanceCommands)
    : IRequestHandler<CreateUserCommand, Result<UserDetailResult>>
{
    public async Task<Result<UserDetailResult>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var validation = await UserValidation.ValidateReferencesAsync(
            unitOfWork,
            organizationLookupQueries,
            request.Email,
            request.RoleId,
            request.TeamId,
            request.TeamleaderId,
            cancellationToken: cancellationToken);
        if (!validation.IsSuccess)
        {
            return Result.Fail<UserDetailResult>(validation.Error);
        }

        var code = await unitOfWork.Users.GenerateUniqueCodeAsync(cancellationToken);
        var createResult = User.Create(
            code,
            request.Name,
            request.HrCode,
            request.Email,
            request.Phone,
            request.Title,
            request.RoleId,
            request.AccountType,
            request.TeamId,
            request.TeamleaderId);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<UserDetailResult>(createResult.Error);
        }

        await unitOfWork.Users.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        await employeeBalanceCommands.InsertForUserAsync(createResult.Value, cancellationToken);

        var user = await userAdminQueries.GetByIdAsync(createResult.Value.Id, cancellationToken);
        return user is null
            ? Result.Fail<UserDetailResult>(IdentityErrors.UserNotFound)
            : Result.Ok(UserDetailResult.From(user));
    }
}
