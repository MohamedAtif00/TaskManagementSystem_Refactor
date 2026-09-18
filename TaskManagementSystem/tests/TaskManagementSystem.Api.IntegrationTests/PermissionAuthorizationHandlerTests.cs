using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class PermissionAuthorizationHandlerTests
{
    [Fact]
    public async Task HandleRequirementAsync_WhenManageHeld_SatisfiesReadRequirement()
    {
        var handler = new PermissionAuthorizationHandler();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(IdentityClaimTypes.Permission, PermissionCodes.Organization.Manage)
        ]));

        var context = new AuthorizationHandlerContext(
            [new PermissionRequirement(PermissionCodes.Organization.Read)],
            user,
            resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenReadHeld_DoesNotSatisfiesCreateRequirement()
    {
        var handler = new PermissionAuthorizationHandler();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(IdentityClaimTypes.Permission, PermissionCodes.Organization.Read)
        ]));

        var context = new AuthorizationHandlerContext(
            [new PermissionRequirement(PermissionCodes.Organization.Create)],
            user,
            resource: null);

        await handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}
