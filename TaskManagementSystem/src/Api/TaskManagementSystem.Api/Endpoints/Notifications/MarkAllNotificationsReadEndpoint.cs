using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Notifications.Features.Notifications.MarkAllNotificationsRead;

namespace TaskManagementSystem.Api.Endpoints.Notifications;

/// <summary>
/// PATCH /notifications/read-all — Marks all notifications as read for the current user.
/// </summary>
public static class MarkAllNotificationsReadEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPatch("/read-all", HandleAsync).RequirePermissionCode(PermissionCodes.Notifications.Update);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(new MarkAllNotificationsReadCommand(userId), cancellationToken);
        return result.ToHttpResult(_ => Results.NoContent());
    }
}
