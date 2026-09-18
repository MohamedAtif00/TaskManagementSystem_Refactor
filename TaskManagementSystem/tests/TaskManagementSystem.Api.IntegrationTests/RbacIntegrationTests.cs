using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.Auth;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class RbacIntegrationTests(TmsWebApplicationFactory factory) : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task AboutMe_WhenAuthenticated_ReturnsRoleNameAndManagePermissions()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/auth/about-me", new { });
        var body = await response.Content.ReadFromJsonAsync<AuthInfoResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Role.Should().Be((int)UserRole.Owner);
        body.RoleName.Should().Be(nameof(UserRole.Owner));
        body.Permissions.Should().Contain(PermissionCodes.Organization.Manage);
        body.Permissions.Should().Contain(PermissionCodes.IdentityRoles.Manage);
        body.Permissions.Should().Contain(PermissionCodes.IdentityUsers.Manage);
    }

    [Fact]
    public async Task ListPermissions_WhenOwner_ReturnsSeededCatalog()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var listResponse = await client.GetAsync("/identity/permissions");
        var permissions = await listResponse.Content.ReadFromJsonAsync<List<PermissionResponse>>();

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        permissions!.Should().Contain(permission => permission.Code == PermissionCodes.Organization.Read);
        permissions.Should().OnlyContain(permission => permission.IsSystem);
        permissions.Select(permission => permission.Code)
            .Should()
            .BeEquivalentTo(PermissionCodes.All);
    }

    [Fact]
    public async Task CreateRole_WithPermissions_ReturnsRoleWithPermissionCodes()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var permissions = await client.GetFromJsonAsync<List<PermissionResponse>>("/identity/permissions");
        var permissionIds = permissions!
            .Where(permission => permission.Code == PermissionCodes.IdentityRoles.Manage)
            .Select(permission => permission.Id)
            .ToArray();

        var response = await client.PostAsJsonAsync(
            "/identity/roles",
            new CreateRoleRequest
            {
                Name = "Auditor",
                Description = "Read-only auditor",
                PermissionIds = permissionIds
            });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var role = await response.Content.ReadFromJsonAsync<RoleResponse>();
        role!.Name.Should().Be("Auditor");
        role.PermissionCodes.Should().Contain(PermissionCodes.IdentityRoles.Manage);
    }

    [Fact]
    public async Task OrganizationRead_WhenOwnerHasManagePermission_Succeeds()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var aboutMeResponse = await client.PostAsJsonAsync("/auth/about-me", new { });
        var aboutMe = await aboutMeResponse.Content.ReadFromJsonAsync<AuthInfoResponse>();
        aboutMe!.Permissions.Should().Contain(PermissionCodes.Organization.Manage);
        aboutMe.Permissions.Should().NotContain(PermissionCodes.Organization.Read);

        var response = await client.GetAsync("/organization/teams");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OrganizationTeams_WhenUnauthenticated_ReturnUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/organization/teams");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IdentityEndpoints_WhenUnauthenticated_ReturnUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/identity/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
