using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Modules.Workflows.Domain;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class WorkflowReorderIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task ReorderNodes_WhenValid_UpdatesOrder()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var schemaResponse = await client.PostAsJsonAsync(
            "/workflows/schemas",
            new CreateSchemaRequest { Name = "Reorder Schema", Description = "Nodes reorder test" });
        var schema = await IntegrationHttpAssertions.EnsureAsync<SchemaDetailResponse>(
            schemaResponse,
            HttpStatusCode.Created);

        var firstNodeResponse = await client.PostAsJsonAsync(
            $"/workflows/schemas/{schema.Id}/nodes",
            new CreateNodeRequest { Name = "First", IsStart = true, IsEnd = false });
        var firstNode = await IntegrationHttpAssertions.EnsureAsync<NodeListItemResponse>(
            firstNodeResponse,
            HttpStatusCode.Created);

        var secondNodeResponse = await client.PostAsJsonAsync(
            $"/workflows/schemas/{schema.Id}/nodes",
            new CreateNodeRequest { Name = "Second", IsStart = false, IsEnd = true });
        var secondNode = await IntegrationHttpAssertions.EnsureAsync<NodeListItemResponse>(
            secondNodeResponse,
            HttpStatusCode.Created);

        var reorderResponse = await client.PutAsJsonAsync(
            $"/workflows/schemas/{schema.Id}/nodes/reorder",
            new ReorderNodesRequest { OrderedNodeIds = [secondNode.Id, firstNode.Id] });
        reorderResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await client.GetAsync($"/workflows/schemas/{schema.Id}/nodes");
        var nodes = await IntegrationHttpAssertions.EnsureAsync<List<NodeListItemResponse>>(
            listResponse,
            HttpStatusCode.OK);

        nodes.Should().HaveCount(2);
        nodes[0].Id.Should().Be(secondNode.Id);
        nodes[0].Order.Should().Be(1);
        nodes[1].Id.Should().Be(firstNode.Id);
        nodes[1].Order.Should().Be(2);
    }

    [Fact]
    public async Task ReorderSteps_WhenValid_UpdatesOrder()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var schemaResponse = await client.PostAsJsonAsync(
            "/workflows/schemas",
            new CreateSchemaRequest { Name = "Step Reorder Schema", Description = "Steps reorder test" });
        var schema = await IntegrationHttpAssertions.EnsureAsync<SchemaDetailResponse>(
            schemaResponse,
            HttpStatusCode.Created);

        var nodeResponse = await client.PostAsJsonAsync(
            $"/workflows/schemas/{schema.Id}/nodes",
            new CreateNodeRequest { Name = "Node", IsStart = true, IsEnd = true });
        var node = await IntegrationHttpAssertions.EnsureAsync<NodeListItemResponse>(
            nodeResponse,
            HttpStatusCode.Created);

        var teamsResponse = await client.GetAsync("/organization/teams");
        var teams = await IntegrationHttpAssertions.EnsureAsync<List<TeamListItemResponse>>(
            teamsResponse,
            HttpStatusCode.OK);
        var team = teams.First();

        var bankOneResponse = await client.PostAsJsonAsync(
            "/workflows/ticket-bank",
            new CreateTicketBankItemRequest
            {
                Name = "Bank A",
                Duration = 30,
                Type = TicketBankType.Creation,
                TeamLeaderOnly = false,
                TeamId = team.Id,
            });
        var bankOne = await IntegrationHttpAssertions.EnsureAsync<TicketBankListItemResponse>(
            bankOneResponse,
            HttpStatusCode.Created);

        var bankTwoResponse = await client.PostAsJsonAsync(
            "/workflows/ticket-bank",
            new CreateTicketBankItemRequest
            {
                Name = "Bank B",
                Duration = 45,
                Type = TicketBankType.Review,
                TeamLeaderOnly = false,
                TeamId = team.Id,
            });
        var bankTwo = await IntegrationHttpAssertions.EnsureAsync<TicketBankListItemResponse>(
            bankTwoResponse,
            HttpStatusCode.Created);

        var firstStepResponse = await client.PostAsJsonAsync(
            $"/workflows/nodes/{node.Id}/steps",
            new CreateStepRequest { TicketBankId = bankOne.Id, Duration = 30, Priority = 2 });
        var firstStep = await IntegrationHttpAssertions.EnsureAsync<StepListItemResponse>(
            firstStepResponse,
            HttpStatusCode.Created);

        var secondStepResponse = await client.PostAsJsonAsync(
            $"/workflows/nodes/{node.Id}/steps",
            new CreateStepRequest { TicketBankId = bankTwo.Id, Duration = 45, Priority = 1 });
        var secondStep = await IntegrationHttpAssertions.EnsureAsync<StepListItemResponse>(
            secondStepResponse,
            HttpStatusCode.Created);

        var reorderResponse = await client.PutAsJsonAsync(
            $"/workflows/nodes/{node.Id}/steps/reorder",
            new ReorderStepsRequest { OrderedStepIds = [secondStep.Id, firstStep.Id] });
        reorderResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await client.GetAsync($"/workflows/nodes/{node.Id}/steps");
        var steps = await IntegrationHttpAssertions.EnsureAsync<List<StepListItemResponse>>(
            listResponse,
            HttpStatusCode.OK);

        steps.Should().HaveCount(2);
        steps[0].Id.Should().Be(secondStep.Id);
        steps[0].Order.Should().Be(1);
        steps[1].Id.Should().Be(firstStep.Id);
        steps[1].Order.Should().Be(2);
    }

    private sealed class TeamListItemResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
