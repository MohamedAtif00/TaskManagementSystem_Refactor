using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.Sprints;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class TicketSummaryIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task GetTicketSummary_BySubject_WhenTicketsExist_ReturnsCounts()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        await client.PostAsJsonAsync(
            "/tickets",
            new CreateTicketRequest
            {
                LearningObjectiveId = setup.LearningObjectiveId,
                TicketBankItemId = setup.TicketBankItemId
            });

        var response = await client.GetAsync($"/tickets/summary?subjectId={setup.SubjectId}");
        var summary = await IntegrationHttpAssertions.EnsureAsync<TicketSummaryResponse>(response, HttpStatusCode.OK);

        summary.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        summary.Backlog.Should().BeGreaterThanOrEqualTo(1);
        summary.CalculatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetTicketSummary_BySubject_WhenNoTickets_ReturnsZeros()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        var response = await client.GetAsync($"/tickets/summary?subjectId={setup.SubjectId}");
        var summary = await IntegrationHttpAssertions.EnsureAsync<TicketSummaryResponse>(response, HttpStatusCode.OK);

        summary.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        summary.Backlog.Should().BeGreaterThanOrEqualTo(0);
        summary.ToDo.Should().BeGreaterThanOrEqualTo(0);
        summary.Doing.Should().BeGreaterThanOrEqualTo(0);
        summary.Done.Should().BeGreaterThanOrEqualTo(0);
        (summary.Backlog + summary.ToDo + summary.Doing + summary.Done).Should().Be(summary.TotalCount);
    }

    [Fact]
    public async Task GetTicketSummary_BySprint_WhenSprintMissing_ReturnsNotFound()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/tickets/summary?sprintId=999999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTicketSummary_BySprint_WhenLinkedTicketsExist_ReturnsCounts()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        var createSprintResponse = await client.PostAsJsonAsync(
            "/sprints",
            new CreateSprintRequest
            {
                Name = "Summary Sprint",
                Description = "Ticket summary tests",
                StartDate = new DateTime(2026, 10, 1),
                EndDate = new DateTime(2026, 10, 31)
            });
        var sprint = await IntegrationHttpAssertions.EnsureAsync<SprintDetailResponse>(
            createSprintResponse,
            HttpStatusCode.Created);

        await client.PostAsJsonAsync(
            $"/sprints/{sprint.Id}/learning-objectives",
            new AddSprintLearningObjectivesRequest { LearningObjectiveIds = [setup.LearningObjectiveId] });

        await client.PostAsJsonAsync(
            "/tickets",
            new CreateTicketRequest
            {
                LearningObjectiveId = setup.LearningObjectiveId,
                TicketBankItemId = setup.TicketBankItemId
            });

        var response = await client.GetAsync($"/tickets/summary?sprintId={sprint.Id}");
        var summary = await IntegrationHttpAssertions.EnsureAsync<TicketSummaryResponse>(response, HttpStatusCode.OK);

        summary.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        summary.Backlog.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetTicketSummary_WhenUnauthenticated_ReturnsUnauthorized()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/tickets/summary?subjectId=1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
