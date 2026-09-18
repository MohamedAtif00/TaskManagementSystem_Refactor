using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.WorkTimes.StartWorkTime;

namespace TaskManagementSystem.Api.Endpoints.Ticket.WorkTimes;

/// <summary>POST /tickets/{id}/work-times/start — start work time on a ticket. Requires Tickets.Create permission.</summary>
public static class StartWorkTimeEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}/work-times/start", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Create);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(new StartWorkTimeCommand(id, userId), cancellationToken);
        return result.ToHttpResult(workTime => Results.Ok(TicketMapping.MapWorkTime(workTime)));
    }
}
