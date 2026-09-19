using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Notifications.Application;

namespace TaskManagementSystem.Modules.Notifications.Features.Notifications.GetNotificationById;

public sealed record GetNotificationByIdQuery(int Id, int UserId) : IQuery<Result<NotificationDetailResult>>;

