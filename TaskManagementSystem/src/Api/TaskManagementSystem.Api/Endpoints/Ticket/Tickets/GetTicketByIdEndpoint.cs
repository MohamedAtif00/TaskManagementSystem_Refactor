using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketById;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

/// <summary>GET /tickets/{id} — get ticket by id. Requires Tickets.Read permission.</summary>
public static class GetTicketByIdEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTicketByIdQuery(id), cancellationToken);
        return result.ToHttpResult(ticket => Results.Ok(TicketMapping.MapTicketDetail(ticket)));
    }
}
