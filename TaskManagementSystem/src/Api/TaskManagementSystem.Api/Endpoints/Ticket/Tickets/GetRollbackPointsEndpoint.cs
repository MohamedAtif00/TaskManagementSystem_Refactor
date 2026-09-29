using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.GetRollbackPoints;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

public static class GetRollbackPointsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}/rollback-points", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRollbackPointsQuery(id), cancellationToken);
        return result.ToHttpResult(points => Results.Ok(points.Select(TicketMapping.MapJumpPoint).ToList()));
    }
}
