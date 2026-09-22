using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketActivity;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

public static class ListTicketActivityEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}/activity", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTicketActivityQuery(id), cancellationToken);
        return result.ToHttpResult(rows => Results.Ok(rows.Select(TicketMapping.MapActivity).ToList()));
    }
}
