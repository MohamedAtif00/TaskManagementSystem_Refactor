using Microsoft.AspNetCore.Authorization;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Security;

public sealed class PermissionRequirement(string permissionCode) : IAuthorizationRequirement
{
    public string PermissionCode { get; } = permissionCode;
}

internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var heldPermissions = context.User.Claims
            .Where(claim => claim.Type == IdentityClaimTypes.Permission)
            .Select(claim => claim.Value);

        foreach (var heldPermission in heldPermissions)
        {
            if (PermissionCodes.IsSatisfiedBy(heldPermission, requirement.PermissionCode))
            {
                context.Succeed(requirement);
                break;
            }
        }

        return Task.CompletedTask;
    }
}
