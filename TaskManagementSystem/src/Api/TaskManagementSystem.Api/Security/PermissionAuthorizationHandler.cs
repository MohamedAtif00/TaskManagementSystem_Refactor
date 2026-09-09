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
        var hasPermission = context.User.Claims.Any(claim =>
            claim.Type == IdentityClaimTypes.Permission &&
            string.Equals(claim.Value, requirement.PermissionCode, StringComparison.Ordinal));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
