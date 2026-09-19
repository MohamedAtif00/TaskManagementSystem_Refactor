using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Identity.Application;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Identity.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Identity.Features.Users;

internal static class UserValidation
{
    public static async Task<Result<NoValue>> ValidateReferencesAsync(
        IIdentityUnitOfWork unitOfWork,
        OrganizationLookupQueries organizationLookupQueries,
        string? email,
        int roleId,
        int? teamId,
        int? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(email)
            && await unitOfWork.Users.ExistsActiveByEmailAsync(email, excludeUserId, cancellationToken))
        {
            return Result.Fail<NoValue>(IdentityErrors.EmailAlreadyExists);
        }

        var role = await unitOfWork.Roles.GetByIdWithPermissionsAsync(roleId, cancellationToken);
        if (role is null)
        {
            return Result.Fail<NoValue>(IdentityErrors.RoleNotFound);
        }

        if (roleId != (int)UserRole.Owner)
        {
            if (teamId is null || !await organizationLookupQueries.ActiveTeamExistsAsync(teamId.Value, cancellationToken))
            {
                return Result.Fail<NoValue>(IdentityErrors.TeamInvalid);
            }
        }

        return Result.Ok();
    }
}
