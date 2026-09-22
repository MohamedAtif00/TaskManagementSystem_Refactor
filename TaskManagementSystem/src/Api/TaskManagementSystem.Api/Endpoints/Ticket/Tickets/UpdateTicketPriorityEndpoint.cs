using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.UpdateTicketPriority;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

public static class UpdateTicketPriorityEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{id:int}/priority", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Update);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        UpdateTicketPriorityRequest request,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(typeof(TicketPriority), request.Priority))
        {
            return Results.BadRequest(new { code = "ticket_invalid_priority", message = "Priority is invalid." });
        }

        var actorUserId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(
            new UpdateTicketPriorityCommand(id, (TicketPriority)request.Priority, actorUserId),
            cancellationToken);
        return result.ToHttpResult(ticket => Results.Ok(TicketMapping.MapTicketDetail(ticket)));
    }
}
