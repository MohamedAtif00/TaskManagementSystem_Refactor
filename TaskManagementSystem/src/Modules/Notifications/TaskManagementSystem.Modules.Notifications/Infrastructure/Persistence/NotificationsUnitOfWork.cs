using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.BuildingBlocks.Persistence.Events;
using TaskManagementSystem.Modules.Notifications.Application;

namespace TaskManagementSystem.Modules.Notifications.Infrastructure.Persistence;

internal sealed class NotificationsUnitOfWork(
    NotificationsDbContext context,
    IDomainEventDispatcher domainEventDispatcher)
    : UnitOfWork<NotificationsDbContext>(context, domainEventDispatcher), INotificationsUnitOfWork
{
    private readonly Lazy<NotificationRepository> _notifications =
        LazyRepositoryFactory.Create(() => new NotificationRepository(context));

    public INotificationRepository Notifications => _notifications.Value;
}
