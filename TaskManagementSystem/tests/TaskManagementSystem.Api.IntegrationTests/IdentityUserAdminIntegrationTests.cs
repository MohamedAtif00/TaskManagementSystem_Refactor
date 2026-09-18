using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.Identity;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class IdentityUserAdminIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task ListUsers_WhenOwner_ReturnsSeededUser()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/identity/users");
        var users = await response.Content.ReadFromJsonAsync<List<UserListItemResponse>>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        users!.Should().Contain(user => user.Code == "TST001");
    }

    [Fact]
    public async Task CreateGetUpdateArchiveUser_WhenOwner_CompletesLifecycle()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var teams = await client.GetFromJsonAsync<List<OrganizationTeamListItem>>("/organization/teams");
        var teamId = teams!.First().Id;

        var createResponse = await client.PostAsJsonAsync(
            "/identity/users",
            new CreateUserRequest
            {
                Name = "Integration Member",
                HrCode = "INT001",
                Email = "integration.member@example.com",
                RoleId = (int)UserRole.Member,
                AccountType = (int)AccountType.Internal,
                TeamId = teamId
            });

        var created = await IntegrationHttpAssertions.EnsureAsync<UserDetailResponse>(
            createResponse,
            HttpStatusCode.Created);
        created!.Name.Should().Be("Integration Member");
        created.Code.Should().HaveLength(6);
        created.TeamId.Should().Be(teamId);

        var getResponse = await client.GetAsync($"/identity/users/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await client.PutAsJsonAsync(
            $"/identity/users/{created.Id}",
            new UpdateUserRequest
            {
                Name = "Integration Member Updated",
                HrCode = "INT001",
                Email = "integration.member@example.com",
                RoleId = (int)UserRole.Member,
                AccountType = (int)AccountType.Internal,
                TeamId = teamId,
                Title = "Developer"
            });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<UserDetailResponse>();
        updated!.Name.Should().Be("Integration Member Updated");
        updated.Title.Should().Be("Developer");

        var archiveResponse = await client.DeleteAsync($"/identity/users/{created.Id}");
        archiveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var missingResponse = await client.GetAsync($"/identity/users/{created.Id}");
        missingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_WhenUnauthenticated_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/identity/users",
            new CreateUserRequest
            {
                Name = "Guest",
                HrCode = "G001",
                RoleId = (int)UserRole.Member,
                AccountType = (int)AccountType.Internal,
                TeamId = 1
            });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed class OrganizationTeamListItem
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}
