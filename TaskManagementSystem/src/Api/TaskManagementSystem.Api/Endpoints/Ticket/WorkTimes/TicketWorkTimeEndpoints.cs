namespace TaskManagementSystem.Api.Endpoints.Ticket.WorkTimes;

public static class TicketWorkTimeEndpoints
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        StartWorkTimeEndpoint.Map(group);
        StopWorkTimeEndpoint.Map(group);
        return group;
    }
}
