using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySprint;

namespace TaskManagementSystem.Api.Endpoints.Ticket.TicketQueries;

/// <summary>GET /sprints/{sprintId}/tickets — list tickets by sprint. Requires Tickets.Read permission.</summary>
public static class ListTicketsBySprintEndpoint
{
    public static WebApplication Map(WebApplication app)
    {
        app.MapGet("/sprints/{sprintId:int}/tickets", HandleAsync)
            .WithTags("Tickets")
            .RequireAuthorization()
            .RequirePermissionCode(PermissionCodes.Tickets.Read);
        return app;
    }

    private static async Task<IResult> HandleAsync(
        int sprintId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTicketsBySprintQuery(sprintId), cancellationToken);
        return result.ToHttpResult(tickets =>
            Results.Ok(tickets.Select(TicketMapping.MapTicketListItem).ToList()));
    }
}
