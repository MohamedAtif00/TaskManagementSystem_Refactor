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
    public async Task AboutMe_WhenAuthenticated_ReturnsRoleNameAndPermissions()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/auth/about-me", new { });
        var body = await response.Content.ReadFromJsonAsync<AuthInfoResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body!.Role.Should().Be((int)UserRole.Owner);
        body.RoleName.Should().Be(nameof(UserRole.Owner));
        body.Permissions.Should().Contain(IdentityPermissionCodes.PermissionsManage);
        body.Permissions.Should().Contain(IdentityPermissionCodes.RolesManage);
        body.Permissions.Should().Contain(IdentityPermissionCodes.UsersAssignRole);
    }

    [Fact]
    public async Task ListPermissions_WhenOwner_ReturnsSeededAndCustomPermissions()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync(
            "/identity/permissions",
            new CreatePermissionRequest
            {
                Code = "reports.view",
                Name = "View reports",
                Description = "Can view reports"
            });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await client.GetAsync("/identity/permissions");
        var permissions = await listResponse.Content.ReadFromJsonAsync<List<PermissionResponse>>();

        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        permissions!.Should().Contain(permission => permission.Code == "reports.view");
        permissions.Should().Contain(permission => permission.Code == IdentityPermissionCodes.PermissionsManage);
    }

    [Fact]
    public async Task DeletePermission_WhenSystemPermission_ReturnsBadRequest()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var permissions = await client.GetFromJsonAsync<List<PermissionResponse>>("/identity/permissions");
        var systemPermission = permissions!.Single(permission => permission.Code == IdentityPermissionCodes.PermissionsManage);

        var response = await client.DeleteAsync($"/identity/permissions/{systemPermission.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRole_WithPermissions_ReturnsRoleWithPermissionCodes()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var permissions = await client.GetFromJsonAsync<List<PermissionResponse>>("/identity/permissions");
        var permissionIds = permissions!
            .Where(permission => permission.Code == IdentityPermissionCodes.RolesManage)
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
        role.PermissionCodes.Should().Contain(IdentityPermissionCodes.RolesManage);
    }

    [Fact]
    public async Task IdentityEndpoints_WhenUnauthenticated_ReturnUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/identity/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
