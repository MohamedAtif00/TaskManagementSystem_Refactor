using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketActivityPaged;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

public static class ListTicketActivityPagedEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}/activity/paged", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Read);
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
            new ListTicketActivityPagedQuery(id, page, pageSize),
            cancellationToken);
        return result.ToHttpResult(pageResult => Results.Ok(TicketMapping.MapActivityListPage(pageResult)));
    }
}
