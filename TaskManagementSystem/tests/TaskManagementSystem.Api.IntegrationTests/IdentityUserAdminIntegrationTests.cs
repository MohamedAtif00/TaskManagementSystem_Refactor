using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
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

        await factory.DrainOutboxesAsync();

        await using var connection = new SqlConnection(factory.ConnectionString);
        var balance = await connection.QuerySingleOrDefaultAsync<EmployeeBalanceRow>(
            """
            SELECT
                [UserId],
                [TeamId],
                [Role],
                [AnnualLeave],
                [AnnualLeaveMax],
                [EmergencyLeave],
                [EmergencyLeaveMax],
                [SickLeave],
                [Permission],
                [PermissionMax],
                [WorkFromHome],
                [WorkFromHomeMax],
                [FromNextBalanceDaysUsed],
                [OldAnnualBalance]
            FROM [hr].[EmployeeBalances]
            WHERE [UserId] = @UserId
            """,
            new { UserId = created.Id });

        balance.Should().NotBeNull();
        balance!.UserId.Should().Be(created.Id);
        balance.TeamId.Should().Be(teamId);
        balance.Role.Should().Be((int)UserRole.Member);
        balance.AnnualLeave.Should().Be(0);
        balance.AnnualLeaveMax.Should().Be(30);
        balance.EmergencyLeave.Should().Be(0);
        balance.EmergencyLeaveMax.Should().Be(5);
        balance.SickLeave.Should().Be(0);
        balance.Permission.Should().Be(0);
        balance.PermissionMax.Should().Be(10);
        balance.WorkFromHome.Should().Be(0);
        balance.WorkFromHomeMax.Should().Be(5);
        balance.FromNextBalanceDaysUsed.Should().Be(0);
        balance.OldAnnualBalance.Should().Be(0);

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

    private sealed class EmployeeBalanceRow
    {
        public int UserId { get; init; }

        public int? TeamId { get; init; }

        public int Role { get; init; }

        public int AnnualLeave { get; init; }

        public int AnnualLeaveMax { get; init; }

        public int EmergencyLeave { get; init; }

        public int EmergencyLeaveMax { get; init; }

        public int SickLeave { get; init; }

        public int Permission { get; init; }

        public int PermissionMax { get; init; }

        public int WorkFromHome { get; init; }

        public int WorkFromHomeMax { get; init; }

        public int FromNextBalanceDaysUsed { get; init; }

        public int OldAnnualBalance { get; init; }
    }
}
