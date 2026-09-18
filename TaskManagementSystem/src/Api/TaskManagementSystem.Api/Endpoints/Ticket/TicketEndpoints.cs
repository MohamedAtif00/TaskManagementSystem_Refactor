using TaskManagementSystem.Api.Endpoints.Ticket.Comments;
using TaskManagementSystem.Api.Endpoints.Ticket.TicketQueries;
using TaskManagementSystem.Api.Endpoints.Ticket.Tickets;
using TaskManagementSystem.Api.Endpoints.Ticket.WorkTimes;

namespace TaskManagementSystem.Api.Endpoints.Ticket;

public static class TicketEndpoints
{
    public static RouteGroupBuilder MapTicketEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/tickets").WithTags("Tickets").RequireAuthorization();

        TicketCrudEndpoints.Map(group);
        TicketCommentEndpoints.Map(group);
        TicketWorkTimeEndpoints.Map(group);

        ListTicketsBySubjectEndpoint.Map(app);
        ListTicketsByLearningObjectiveEndpoint.Map(app);
        ListTicketsBySprintEndpoint.Map(app);

        return group;
    }
}
