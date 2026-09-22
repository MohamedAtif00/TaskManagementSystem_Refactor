using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.GetJumpPoints;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

public static class GetJumpPointsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:int}/jump-points", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetJumpPointsQuery(id), cancellationToken);
        return result.ToHttpResult(points => Results.Ok(points.Select(TicketMapping.MapJumpPoint).ToList()));
    }
}
