using Microsoft.AspNetCore.Authorization;
using TaskManagementSystem.Modules.Identity.Domain;

namespace TaskManagementSystem.Api.Security;

public sealed class PermissionRequirement(string permissionCode) : IAuthorizationRequirement
{
    public string PermissionCode { get; } = permissionCode;
}

public sealed class AnyPermissionRequirement(params string[] permissionCodes) : IAuthorizationRequirement
{
    public IReadOnlyList<string> PermissionCodes { get; } = permissionCodes;
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

internal sealed class AnyPermissionAuthorizationHandler : AuthorizationHandler<AnyPermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionRequirement requirement)
    {
        var heldPermissions = context.User.Claims
            .Where(claim => claim.Type == IdentityClaimTypes.Permission)
            .Select(claim => claim.Value);

        foreach (var requiredCode in requirement.PermissionCodes)
        {
            foreach (var heldPermission in heldPermissions)
            {
                if (PermissionCodes.IsSatisfiedBy(heldPermission, requiredCode))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
        }

        return Task.CompletedTask;
    }
}
