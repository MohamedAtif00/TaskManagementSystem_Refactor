using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.Modules.Notifications.Application;

public interface INotificationsUnitOfWork : IUnitOfWork
{
    INotificationRepository Notifications { get; }
}
