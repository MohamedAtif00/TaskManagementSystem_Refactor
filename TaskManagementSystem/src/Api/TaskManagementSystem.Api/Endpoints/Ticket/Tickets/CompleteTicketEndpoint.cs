using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.CompleteTicket;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

/// <summary>PATCH /tickets/{id}/complete — complete a ticket. Requires Tickets.Update permission.</summary>
public static class CompleteTicketEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{id:int}/complete", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Update);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CompleteTicketCommand(id, currentUserAccessor.GetRequiredUserId(), currentUserAccessor.GetRequiredRole()),
            cancellationToken);
        return result.ToHttpResult(ticket => Results.Ok(TicketMapping.MapTicketDetail(ticket)));
    }
}
