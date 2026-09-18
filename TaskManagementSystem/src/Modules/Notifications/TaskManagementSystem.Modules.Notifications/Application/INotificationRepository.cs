using TaskManagementSystem.Modules.Notifications.Domain;

namespace TaskManagementSystem.Modules.Notifications.Application;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdTrackedAsync(int id, int userId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Notification> Items, int TotalCount)> ListByUserAsync(
        int userId,
        bool? isRead,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    Task MarkAllReadAsync(int userId, CancellationToken cancellationToken = default);
}
