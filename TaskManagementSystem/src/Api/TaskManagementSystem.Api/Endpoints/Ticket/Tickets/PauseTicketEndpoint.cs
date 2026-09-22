using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.PauseTicket;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

public static class PauseTicketEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{id:int}/pause", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Update);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new PauseTicketCommand(id), cancellationToken);
        return result.ToHttpResult(ticket => Results.Ok(TicketMapping.MapTicketDetail(ticket)));
    }
}
