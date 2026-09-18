using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Notifications.Features.Notifications.MarkNotificationRead;

namespace TaskManagementSystem.Api.Endpoints.Notifications;

/// <summary>
/// PATCH /notifications/{id}/read — Marks a notification as read.
/// </summary>
public static class MarkNotificationReadEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{id:int}/read", HandleAsync).RequirePermissionCode(PermissionCodes.Notifications.Update);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(new MarkNotificationReadCommand(id, userId), cancellationToken);
        return result.ToHttpResult(notification => Results.Ok(NotificationMapping.MapNotificationDetail(notification)));
    }
}
