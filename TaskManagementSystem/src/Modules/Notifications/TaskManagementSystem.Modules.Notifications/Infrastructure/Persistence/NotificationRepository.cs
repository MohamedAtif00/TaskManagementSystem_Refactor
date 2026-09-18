using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Notifications.Application;
using TaskManagementSystem.Modules.Notifications.Domain;

namespace TaskManagementSystem.Modules.Notifications.Infrastructure.Persistence;

internal sealed class NotificationRepository(NotificationsDbContext context)
    : GenericRepository<Notification, NotificationsDbContext>(context), INotificationRepository
{
    public Task<Notification?> GetByIdAsync(int id, int userId, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(notification => notification.Id == id && notification.UserId == userId, cancellationToken);

    public Task<Notification?> GetByIdTrackedAsync(int id, int userId, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(notification => notification.Id == id && notification.UserId == userId, cancellationToken);

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> ListByUserAsync(
        int userId,
        bool? isRead,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking().Where(notification => notification.UserId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(notification => notification.IsRead == isRead.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(notification => notification.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task AddAsync(Notification notification, CancellationToken cancellationToken = default) =>
        AddEntityAsync(notification, cancellationToken);

    public async Task MarkAllReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        var unread = await Set
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
        {
            notification.MarkRead();
        }
    }
}
