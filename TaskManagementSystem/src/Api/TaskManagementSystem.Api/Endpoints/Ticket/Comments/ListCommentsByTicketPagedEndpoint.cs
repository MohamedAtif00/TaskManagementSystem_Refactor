using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Comments.ListCommentsByTicketPaged;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Comments;

/// <summary>GET /tickets/{id}/comments/paged — paged comments for a ticket.</summary>
public static class ListCommentsByTicketPagedEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}/comments/paged", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        int? page,
        int? pageSize,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListCommentsByTicketPagedQuery(id, page, pageSize),
            cancellationToken);
        return result.ToHttpResult(pageResult => Results.Ok(TicketMapping.MapCommentListPage(pageResult)));
    }
}
