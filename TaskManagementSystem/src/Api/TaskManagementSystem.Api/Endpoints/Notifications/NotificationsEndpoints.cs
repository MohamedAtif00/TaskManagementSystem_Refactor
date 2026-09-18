namespace TaskManagementSystem.Api.Endpoints.Notifications;

public static class NotificationsEndpoints
{
    public static RouteGroupBuilder MapNotificationsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/notifications").WithTags("Notifications").RequireAuthorization();

        ListMyNotificationsEndpoint.Map(group);
        GetNotificationByIdEndpoint.Map(group);
        MarkNotificationReadEndpoint.Map(group);
        MarkAllNotificationsReadEndpoint.Map(group);

        return group;
    }
}
