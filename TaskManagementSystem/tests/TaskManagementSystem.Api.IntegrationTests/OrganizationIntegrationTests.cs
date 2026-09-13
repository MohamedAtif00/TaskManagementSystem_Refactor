using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.Auth;
using TaskManagementSystem.Api.Contracts.Organization;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class OrganizationIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task TeamFlow_WhenCreatedUpdatedAndArchived_Succeeds()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync(
            "/organization/teams",
            new CreateTeamRequest { Name = "New Platform Team" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TeamListItemResponse>();
        created!.Name.Should().Be("New Platform Team");
        created.Members.Should().Be(0);

        var updateResponse = await client.PutAsJsonAsync(
            $"/organization/teams/{created.Id}",
            new UpdateTeamRequest { Name = "Renamed Platform Team" });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TeamListItemResponse>();
        updated!.Name.Should().Be("Renamed Platform Team");

        var listResponse = await client.GetAsync("/organization/teams");
        var teams = await listResponse.Content.ReadFromJsonAsync<List<TeamListItemResponse>>();
        teams!.Should().Contain(team => team.Id == created.Id && team.Name == "Renamed Platform Team");

        var deleteResponse = await client.DeleteAsync($"/organization/teams/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/organization/teams/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SectionFlow_WhenCreatedWithTeams_ReturnsHeadAndLinkedTeams()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var teamsResponse = await client.GetAsync("/organization/teams");
        var teams = await teamsResponse.Content.ReadFromJsonAsync<List<TeamListItemResponse>>();
        var seededTeam = teams!.Single(team => team.Name == IntegrationTestDataSeeder.TestTeamName);

        var aboutMeResponse = await client.PostAsJsonAsync("/auth/about-me", new { });
        aboutMeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var aboutMe = await aboutMeResponse.Content.ReadFromJsonAsync<AuthInfoResponse>();

        var createResponse = await client.PostAsJsonAsync(
            "/organization/sections",
            new CreateSectionRequest
            {
                Name = "Integration Section",
                HeadId = aboutMe!.Id,
                TeamIds = [seededTeam.Id]
            });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<SectionDetailResponse>();
        created!.Name.Should().Be("Integration Section");
        created.Teams.Should().ContainSingle(team => team.Id == seededTeam.Id);

        var duplicateResponse = await client.PostAsJsonAsync(
            "/organization/sections",
            new CreateSectionRequest
            {
                Name = "Integration Section",
                HeadId = aboutMe.Id,
                TeamIds = [seededTeam.Id]
            });
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var deleteResponse = await client.DeleteAsync($"/organization/sections/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
