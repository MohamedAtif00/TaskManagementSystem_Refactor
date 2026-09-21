namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

public static class TicketCrudEndpoints
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        CreateTicketEndpoint.Map(group);
        GetTicketByIdEndpoint.Map(group);
        AssignTicketEndpoint.Map(group);
        ProceedTicketEndpoint.Map(group);
        CompleteTicketEndpoint.Map(group);
        FlagTicketEndpoint.Map(group);
        RollbackTicketEndpoint.Map(group);
        GetTicketStatsEndpoint.Map(group);
        return group;
    }
}
