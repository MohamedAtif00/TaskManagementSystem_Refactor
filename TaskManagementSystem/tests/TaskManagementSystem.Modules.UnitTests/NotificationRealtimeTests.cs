using FluentAssertions;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Notifications.Contracts;
using TaskManagementSystem.TestCommon.Builders;
using Xunit;

namespace TaskManagementSystem.Modules.UnitTests;

public sealed class NotificationRealtimeTests
{
    [Fact]
    public void Visible_SetsNotificationKindAndEventName()
    {
        var payload = new NotificationPayloadBuilder()
            .WithTitle("Task assigned")
            .WithBody("You have a new ticket.")
            .Build();

        var message = NotificationRealtime.Visible(payload);

        message.EventName.Should().Be(NotificationRealtime.Created);
        message.Kind.Should().Be(RealtimeDeliveryKind.Notification);
        message.Payload.Should().BeSameAs(payload);
    }
}
