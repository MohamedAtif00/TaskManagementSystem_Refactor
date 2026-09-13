using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagementSystem.Api.Contracts.Organization;
using TaskManagementSystem.Api.Contracts.Workflows;
using TaskManagementSystem.Modules.Workflows.Domain;
using TaskManagementSystem.TestCommon.Integration;
using Xunit;

namespace TaskManagementSystem.Api.IntegrationTests;

public sealed class WorkflowsIntegrationTests(TmsWebApplicationFactory factory)
    : IClassFixture<TmsWebApplicationFactory>
{
    [Fact]
    public async Task SchemaAndNodeFlow_WhenCreatedUpdatedAndArchived_Succeeds()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var createSchemaResponse = await client.PostAsJsonAsync(
            "/workflows/schemas",
            new CreateSchemaRequest
            {
                Name = "Integration Schema",
                Description = "Workflow for integration tests"
            });
        createSchemaResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var schema = await createSchemaResponse.Content.ReadFromJsonAsync<SchemaDetailResponse>();
        schema!.Name.Should().Be("Integration Schema");

        var createNodeResponse = await client.PostAsJsonAsync(
            $"/workflows/schemas/{schema.Id}/nodes",
            new CreateNodeRequest { Name = "Start", IsStart = true, IsEnd = false });
        createNodeResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var node = await createNodeResponse.Content.ReadFromJsonAsync<NodeListItemResponse>();
        node!.Order.Should().Be(1);
        node.IsStart.Should().BeTrue();

        var listNodesResponse = await client.GetAsync($"/workflows/schemas/{schema.Id}/nodes");
        var nodes = await listNodesResponse.Content.ReadFromJsonAsync<List<NodeListItemResponse>>();
        nodes!.Should().ContainSingle(item => item.Id == node.Id);

        var updateNodeResponse = await client.PutAsJsonAsync(
            $"/workflows/nodes/{node.Id}",
            new UpdateNodeRequest { Name = "Updated Start", IsStart = true, IsEnd = false });
        updateNodeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteNodeResponse = await client.DeleteAsync($"/workflows/nodes/{node.Id}");
        deleteNodeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deleteSchemaResponse = await client.DeleteAsync($"/workflows/schemas/{schema.Id}");
        deleteSchemaResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task TaskBankAndStepFlow_WhenLinkedToTeamAndNode_Succeeds()
    {
        var client = await factory.CreateAuthenticatedClientAsync();

        var teamsResponse = await client.GetAsync("/organization/teams");
        var teams = await teamsResponse.Content.ReadFromJsonAsync<List<TeamListItemResponse>>();
        var seededTeam = teams!.Single(team => team.Name == IntegrationTestDataSeeder.TestTeamName);

        var createTaskBankResponse = await client.PostAsJsonAsync(
            "/workflows/task-bank",
            new CreateTaskBankItemRequest
            {
                Name = "Integration Task",
                Duration = 45,
                Type = TaskBankType.Creation,
                TeamLeaderOnly = false,
                TeamId = seededTeam.Id
            });
        createTaskBankResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var taskBankItem = await createTaskBankResponse.Content.ReadFromJsonAsync<TaskBankListItemResponse>();

        var createSchemaResponse = await client.PostAsJsonAsync(
            "/workflows/schemas",
            new CreateSchemaRequest { Name = "Step Schema", Description = "For step tests" });
        var schema = await createSchemaResponse.Content.ReadFromJsonAsync<SchemaDetailResponse>();

        var createNodeResponse = await client.PostAsJsonAsync(
            $"/workflows/schemas/{schema!.Id}/nodes",
            new CreateNodeRequest { Name = "Work", IsStart = true, IsEnd = false });
        var node = await createNodeResponse.Content.ReadFromJsonAsync<NodeListItemResponse>();

        var createStepResponse = await client.PostAsJsonAsync(
            $"/workflows/nodes/{node!.Id}/steps",
            new CreateStepRequest
            {
                TaskBankId = taskBankItem!.Id,
                Duration = 30,
                Priority = 1
            });
        createStepResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var step = await createStepResponse.Content.ReadFromJsonAsync<StepListItemResponse>();
        step!.Order.Should().Be(1);
        step.TaskBankId.Should().Be(taskBankItem.Id);

        var listStepsResponse = await client.GetAsync($"/workflows/nodes/{node.Id}/steps");
        var steps = await listStepsResponse.Content.ReadFromJsonAsync<List<StepListItemResponse>>();
        steps!.Should().ContainSingle(item => item.Id == step.Id);

        var deleteStepResponse = await client.DeleteAsync($"/workflows/steps/{step.Id}");
        deleteStepResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deleteTaskBankResponse = await client.DeleteAsync($"/workflows/task-bank/{taskBankItem.Id}");
        deleteTaskBankResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
