using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Notifications.Features.Notifications.GetNotificationById;

namespace TaskManagementSystem.Api.Endpoints.Notifications;

/// <summary>
/// GET /notifications/{id} — Gets a notification by ID.
/// </summary>
public static class GetNotificationByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Notifications.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(new GetNotificationByIdQuery(id, userId), cancellationToken);
        return result.ToHttpResult(notification => Results.Ok(NotificationMapping.MapNotificationDetail(notification)));
    }
}
