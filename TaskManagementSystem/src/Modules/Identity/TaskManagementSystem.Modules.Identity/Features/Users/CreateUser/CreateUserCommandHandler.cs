using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Identity.Features.Users.CreateUser;

public sealed class CreateUserCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    IUserAdminQueries userAdminQueries,
    OrganizationLookupQueries organizationLookupQueries)
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
            request.TeamId);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<UserDetailResult>(createResult.Error);
        }

        var userEntity = createResult.Value;
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await unitOfWork.Users.AddAsync(userEntity, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            userEntity.NotifyCreated();
            await unitOfWork.CommitAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        var user = await userAdminQueries.GetByIdAsync(userEntity.Id, cancellationToken)
            ?? await userAdminQueries.GetByCodeAsync(userEntity.Code, cancellationToken);
        return user is null
            ? Result.Fail<UserDetailResult>(IdentityErrors.UserNotFound)
            : Result.Ok(UserDetailResult.From(user));
    }
}
