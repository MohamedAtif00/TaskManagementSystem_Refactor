using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.AssignTicket;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

/// <summary>PATCH /tickets/{id}/assign — assign a ticket. Requires Tickets.Update permission.</summary>
public static class AssignTicketEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{id:int}/assign", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Update);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        AssignTicketRequest request,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new AssignTicketCommand(
                id,
                request.UserId,
                currentUserAccessor.GetRequiredUserId(),
                currentUserAccessor.GetRequiredRole()),
            cancellationToken);
        return result.ToHttpResult(ticket => Results.Ok(TicketMapping.MapTicketDetail(ticket)));
    }
}
