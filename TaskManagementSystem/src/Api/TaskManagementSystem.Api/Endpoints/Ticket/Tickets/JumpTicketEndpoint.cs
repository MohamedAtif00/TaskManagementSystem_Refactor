using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.JumpTicket;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

public static class JumpTicketEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{id:int}/jump", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Update);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        JumpTicketRequest request,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var actorUserId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(
            new JumpTicketCommand(id, request.StepId, actorUserId, currentUserAccessor.GetRequiredRole()),
            cancellationToken);
        return result.ToHttpResult(ticket => Results.Ok(TicketMapping.MapTicketDetail(ticket)));
    }
}
