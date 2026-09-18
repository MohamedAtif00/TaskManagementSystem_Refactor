using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Notifications.Features.Notifications.ListMyNotifications;

namespace TaskManagementSystem.Api.Endpoints.Notifications;

/// <summary>
/// GET /notifications — Lists notifications for the current user.
/// </summary>
public static class ListMyNotificationsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("", HandleAsync).RequirePermissionCode(PermissionCodes.Notifications.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        bool? isRead,
        int? page,
        int? pageSize,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(
            new ListMyNotificationsQuery(userId, isRead, page ?? 1, pageSize ?? 20),
            cancellationToken);

        return result.ToHttpResult(pageResult => Results.Ok(NotificationMapping.MapNotificationListPage(pageResult)));
    }
}
