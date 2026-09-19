using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Notifications.Application;

namespace TaskManagementSystem.Modules.Notifications.Features.Notifications.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(int Id, int UserId) : ICommand<Result<NotificationDetailResult>>;

