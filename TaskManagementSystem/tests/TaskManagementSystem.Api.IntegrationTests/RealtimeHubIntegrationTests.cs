using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using TaskManagementSystem.Api.Hubs;
using TaskManagementSystem.Api.IntegrationTests;
using TaskManagementSystem.TestCommon.Integration;

namespace TaskManagementSystem.Api.IntegrationTests;

[Collection(ApiIntegrationTestCollection.Name)]
public sealed class RealtimeHubIntegrationTests(TmsWebApplicationFactory factory)
{
    [Fact]
    public async Task ConnectAndJoinGroup_WhenHubIsAvailable_CompletesWithoutError()
    {
        var connection = await CreateConnectionAsync();

        await connection.InvokeAsync("JoinGroup", "team-integration");
        await connection.InvokeAsync("LeaveGroup", "team-integration");

        connection.State.Should().Be(HubConnectionState.Connected);
        await connection.DisposeAsync();
    }

    private async Task<HubConnection> CreateConnectionAsync()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(factory.Server.BaseAddress!, RealtimeHub.Path), options =>
            {
                options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
            })
            .Build();

        await connection.StartAsync();
        return connection;
    }
}
