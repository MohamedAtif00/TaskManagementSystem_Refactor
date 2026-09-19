using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Notifications.Application;

namespace TaskManagementSystem.Modules.Notifications.Features.Notifications.MarkAllNotificationsRead;

public sealed record MarkAllNotificationsReadCommand(int UserId) : ICommand<Result<NoValue>>;

