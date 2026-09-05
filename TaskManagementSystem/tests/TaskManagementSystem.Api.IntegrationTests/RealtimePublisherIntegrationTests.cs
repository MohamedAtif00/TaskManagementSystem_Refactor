using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TaskManagementSystem.Api.Hubs;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Contracts;
using TaskManagementSystem.TestCommon.Builders;
using TaskManagementSystem.Api.IntegrationTests;
using TaskManagementSystem.TestCommon.Integration;

namespace TaskManagementSystem.Api.IntegrationTests;

[Collection(ApiIntegrationTestCollection.Name)]
public sealed class RealtimePublisherIntegrationTests(TmsWebApplicationFactory factory)
{
    [Fact]
    public async Task PublishToGroupAsync_WhenClientJoinedGroup_DeliversSilentTicketUpdate()
    {
        const string groupName = "team-publisher";
        var payload = new TicketPayloadBuilder()
            .WithTicketId(99)
            .WithTitle("Board refresh")
            .WithStatus("InProgress")
            .Build();

        var messageReceived = new TaskCompletionSource<RealtimeMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var connection = await CreateConnectionAsync();

        connection.On<RealtimeMessage>(TicketRealtime.Updated, message =>
        {
            messageReceived.TrySetResult(message);
        });

        await connection.InvokeAsync("JoinGroup", groupName);

        using var scope = factory.Services.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IRealtimePublisher>();
        await publisher.PublishToGroupAsync(groupName, TicketRealtime.SilentUpdate(payload));

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var received = await messageReceived.Task.WaitAsync(timeout.Token);

        received.EventName.Should().Be(TicketRealtime.Updated);
        received.Kind.Should().Be(RealtimeDeliveryKind.Silent);

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var deserializedPayload = ((JsonElement)received.Payload!).Deserialize<TicketUpdatePayload>(jsonOptions);
        deserializedPayload.Should().BeEquivalentTo(payload);

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
