using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Identity.Features.Users.UpdateUser;

public sealed record UpdateUserCommand(
    int UserId,
    string Name,
    string HrCode,
    string? Email,
    string? Phone,
    string? Title,
    int RoleId,
    AccountType AccountType,
    int? TeamId,
    int? TeamleaderId) : ICommand<Result<UserDetailResult>>;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.HrCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Title).MaximumLength(100);
        RuleFor(x => x.RoleId).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateUserCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    IUserAdminQueries userAdminQueries,
    OrganizationLookupQueries organizationLookupQueries,
    EmployeeBalanceCommands employeeBalanceCommands)
    : IRequestHandler<UpdateUserCommand, Result<UserDetailResult>>
{
    public async Task<Result<UserDetailResult>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Users.GetByIdTrackedAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Fail<UserDetailResult>(IdentityErrors.UserNotFound);
        }

        var validation = await UserValidation.ValidateReferencesAsync(
            unitOfWork,
            organizationLookupQueries,
            request.Email,
            request.RoleId,
            request.TeamId,
            request.TeamleaderId,
            request.UserId,
            cancellationToken);
        if (!validation.IsSuccess)
        {
            return Result.Fail<UserDetailResult>(validation.Error);
        }

        var updateResult = user.UpdateProfile(
            request.Name,
            request.HrCode,
            request.Email,
            request.Phone,
            request.Title,
            request.RoleId,
            request.AccountType,
            request.TeamId,
            request.TeamleaderId);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<UserDetailResult>(updateResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await employeeBalanceCommands.SyncMetadataAsync(user, cancellationToken);

        var detail = await userAdminQueries.GetByIdAsync(user.Id, cancellationToken);
        return detail is null
            ? Result.Fail<UserDetailResult>(IdentityErrors.UserNotFound)
            : Result.Ok(UserDetailResult.From(detail));
    }
}
