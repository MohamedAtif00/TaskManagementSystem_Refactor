namespace TaskManagementSystem.Api.Endpoints.Analytics;

public static class AnalyticsEndpoints
{
    public static RouteGroupBuilder MapAnalyticsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/analytics").WithTags("Analytics").RequireAuthorization();

        GetSubjectOverviewEndpoint.Map(group);
        GetSprintOverviewEndpoint.Map(group);

        return group;
    }
}
