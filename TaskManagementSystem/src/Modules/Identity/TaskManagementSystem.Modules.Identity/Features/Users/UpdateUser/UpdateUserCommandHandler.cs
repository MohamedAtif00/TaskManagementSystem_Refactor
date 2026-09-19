using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Identity.Features.Users.UpdateUser;

public sealed class UpdateUserCommandHandler(
    IIdentityUnitOfWork unitOfWork,
    IUserAdminQueries userAdminQueries,
    OrganizationLookupQueries organizationLookupQueries)
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
            request.TeamId);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<UserDetailResult>(updateResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        var detail = await userAdminQueries.GetByIdAsync(user.Id, cancellationToken);
        return detail is null
            ? Result.Fail<UserDetailResult>(IdentityErrors.UserNotFound)
            : Result.Ok(UserDetailResult.From(detail));
    }
}
