using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.IntegrationEvents.Ticket;
using TaskManagementSystem.Modules.Notifications.Application;
using TaskManagementSystem.Modules.Notifications.Domain;
using TaskManagementSystem.Modules.Notifications.Features.Integration;
using Xunit;

namespace TaskManagementSystem.Modules.Notifications.UnitTests;

public sealed class OnTicketAssignedIntegrationEventTests
{
    [Fact]
    public async Task Handle_WhenRealtimePushFails_StillCompletesAfterCommit()
    {
        var integrationEvent = new TicketAssignedIntegrationEvent(Guid.NewGuid(), DateTime.UtcNow, 42, 99);

        var inboxGuard = Substitute.For<IInboxGuard>();
        inboxGuard.TryBeginAsync(
                integrationEvent.Id,
                "Notifications.OnTicketAssigned",
                integrationEvent,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var unitOfWork = Substitute.For<INotificationsUnitOfWork>();
        unitOfWork.Notifications.AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        unitOfWork.CommitAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var realtimePublisher = Substitute.For<IRealtimePublisher>();
        realtimePublisher.PublishToUserAsync(
                Arg.Any<string>(),
                Arg.Any<RealtimeMessage>(),
                Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("signalr unavailable"));

        var handler = new OnTicketAssignedIntegrationEvent(
            inboxGuard,
            unitOfWork,
            realtimePublisher,
            NullLogger<OnTicketAssignedIntegrationEvent>.Instance);

        var act = () => handler.Handle(integrationEvent, CancellationToken.None);

        await act.Should().NotThrowAsync();
        await unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
        await realtimePublisher.Received(1).PublishToUserAsync(
            "99",
            Arg.Any<RealtimeMessage>(),
            Arg.Any<CancellationToken>());
    }
}
