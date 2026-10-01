using System.Security.Claims;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Security;

public static class PermissionClaimChecks
{
    public static bool Satisfies(ClaimsPrincipal user, string permissionCode) =>
        user.Claims
            .Where(claim => claim.Type == IdentityClaimTypes.Permission)
            .Any(claim => PermissionCodes.IsSatisfiedBy(claim.Value, permissionCode));

    /// <summary>
    /// Readers keep their role scope. Create-only callers see their own rows.
    /// </summary>
    public static string ListRole(ICurrentUserAccessor currentUser, ClaimsPrincipal user, string readCode) =>
        Satisfies(user, readCode) ? currentUser.GetRequiredRole() : "Member";
}
