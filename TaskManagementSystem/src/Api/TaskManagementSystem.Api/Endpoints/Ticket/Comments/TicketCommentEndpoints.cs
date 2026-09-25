namespace TaskManagementSystem.Api.Endpoints.Ticket.Comments;

public static class TicketCommentEndpoints
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        AddCommentEndpoint.Map(group);
        UpdateCommentEndpoint.Map(group);
        DeleteCommentEndpoint.Map(group);
        ListCommentsByTicketEndpoint.Map(group);
        ListCommentsByTicketPagedEndpoint.Map(group);
        return group;
    }
}
