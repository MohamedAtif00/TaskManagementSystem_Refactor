namespace TaskManagementSystem.Api.Endpoints.Ticket.Comments;

public static class TicketCommentEndpoints
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        AddCommentEndpoint.Map(group);
        ListCommentsByTicketEndpoint.Map(group);
        return group;
    }
}
