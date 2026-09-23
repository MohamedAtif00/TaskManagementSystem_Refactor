using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.RollbackTicket;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

public static class RollbackTicketEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{id:int}/rollback", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Update);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RollbackTicketCommand(id, currentUserAccessor.GetRequiredUserId(), currentUserAccessor.GetRequiredRole()),
            cancellationToken);
        return result.ToHttpResult(ticket => Results.Ok(TicketMapping.MapTicketDetail(ticket)));
    }
}
