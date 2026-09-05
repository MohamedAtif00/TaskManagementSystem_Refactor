using FluentAssertions;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Contracts;
using TaskManagementSystem.TestCommon.Builders;
using Xunit;

namespace TaskManagementSystem.Modules.UnitTests;

public sealed class TicketRealtimeTests
{
    [Fact]
    public void SilentUpdate_SetsSilentKindAndEventName()
    {
        var payload = new TicketPayloadBuilder()
            .WithTicketId(42)
            .WithTitle("Update board")
            .WithStatus("Done")
            .Build();

        var message = TicketRealtime.SilentUpdate(payload);

        message.EventName.Should().Be(TicketRealtime.Updated);
        message.Kind.Should().Be(RealtimeDeliveryKind.Silent);
        message.Payload.Should().BeSameAs(payload);
    }
}
