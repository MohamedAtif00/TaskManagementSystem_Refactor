using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Modules.Ticket.Domain;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class TicketPaginationIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task ListBySubject_WhenPageSizeExceedsMax_Returns400()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        var response = await client.GetAsync($"/subjects/{setup.SubjectId}/tickets?page=1&pageSize=101");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListBySubject_WhenPageIsZero_Returns400()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        var response = await client.GetAsync($"/subjects/{setup.SubjectId}/tickets?page=0&pageSize=20");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListBySubject_WhenPageSizeIsZero_Returns400()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        var response = await client.GetAsync($"/subjects/{setup.SubjectId}/tickets?page=1&pageSize=0");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListBySubject_WhenPageBeyondData_ReturnsEmptyItems()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);

        var response = await client.GetAsync($"/subjects/{setup.SubjectId}/tickets?page=9999&pageSize=20");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<TicketListPageResponse>();
        page!.Items.Should().BeEmpty();
        page.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ListBySubject_WhenUnauthenticated_Returns401()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/subjects/1/tickets?page=1&pageSize=20");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListBySubject_WithIdenticalTimestamps_HasNoDuplicatesAcrossPages()
    {
        var client = await factory.CreateAuthenticatedClientAsync();
        var setup = await TicketIntegrationTests.CreateTicketSetupAsync(client);
        var sharedTimestamp = DateTime.UtcNow;

        await using var connection = new SqlConnection(factory.ConnectionString);
        for (var index = 0; index < 5; index++)
        {
            var createResponse = await client.PostAsJsonAsync(
                "/tickets",
                new CreateTicketRequest
                {
                    LearningObjectiveId = setup.LearningObjectiveId,
                    TicketBankItemId = setup.TicketBankItemId
                });
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var ticket = await createResponse.Content.ReadFromJsonAsync<TicketDetailResponse>();
            await connection.ExecuteAsync(
                """
                UPDATE [ticket].[Tickets]
                SET [CreatedAt] = @CreatedAt
                WHERE [Id] = @Id
                """,
                new { CreatedAt = sharedTimestamp, Id = ticket!.Id });
        }

        var collected = new List<int>();
        for (var page = 1; page <= 3; page++)
        {
            var response = await client.GetAsync(
                $"/subjects/{setup.SubjectId}/tickets?page={page}&pageSize=2");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var pageResult = await response.Content.ReadFromJsonAsync<TicketListPageResponse>();
            pageResult!.PageSize.Should().Be(2);
            collected.AddRange(pageResult.Items.Select(item => item.Id));
        }

        collected.Should().OnlyHaveUniqueItems();
        collected.Count.Should().BeGreaterThanOrEqualTo(5);
    }
}
