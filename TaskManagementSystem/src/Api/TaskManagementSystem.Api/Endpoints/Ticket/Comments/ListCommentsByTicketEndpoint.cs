using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Comments.ListCommentsByTicket;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Comments;

/// <summary>GET /tickets/{id}/comments — list comments for a ticket. Requires Tickets.Read permission.</summary>
public static class ListCommentsByTicketEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}/comments", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListCommentsByTicketQuery(id), cancellationToken);
        return result.ToHttpResult(comments =>
            Results.Ok(comments.Select(TicketMapping.MapComment).ToList()));
    }
}
