using MediatR;
using Microsoft.Extensions.Logging;
using TaskManagementSystem.BuildingBlocks.Application.Inbox;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.IntegrationEvents.Ticket;
using TaskManagementSystem.Modules.Notifications.Application;
using TaskManagementSystem.Modules.Notifications.Contracts;
using TaskManagementSystem.Modules.Notifications.Domain;

namespace TaskManagementSystem.Modules.Notifications.Features.Integration;

internal sealed class OnTicketCompletedIntegrationEvent(
    IInboxGuard inboxGuard,
    INotificationsUnitOfWork unitOfWork,
    IRealtimePublisher realtimePublisher,
    ILogger<OnTicketCompletedIntegrationEvent> logger)
    : INotificationHandler<TicketCompletedIntegrationEvent>
{
    private const string ConsumerName = "Notifications.OnTicketCompleted";

    public async Task Handle(TicketCompletedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        if (!await inboxGuard.TryBeginAsync(notification.Id, ConsumerName, notification, cancellationToken))
        {
            return;
        }

        var createResult = Notification.Create(
            notification.AssignedUserId,
            "Task completed",
            "A task assigned to you has been completed.",
            "Task",
            "Completion",
            notification.TicketId);

        if (!createResult.IsSuccess)
        {
            throw new InvalidOperationException(createResult.Error.Message);
        }

        var createdNotification = createResult.Value;
        await unitOfWork.Notifications.AddAsync(createdNotification, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var payload = new
        {
            createdNotification.Id,
            createdNotification.UserId,
            createdNotification.Title,
            Message = createdNotification.Message,
            createdNotification.Category,
            createdNotification.Type,
            createdNotification.IsRead,
            createdNotification.CreatedAt,
            createdNotification.RelatedEntityId
        };

        try
        {
            await realtimePublisher.PublishToUserAsync(
                notification.AssignedUserId.ToString(),
                NotificationRealtime.Visible(payload),
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                exception,
                "Notification {NotificationId} was persisted but the realtime push to user {UserId} failed.",
                createdNotification.Id,
                createdNotification.UserId);
        }
    }
}
