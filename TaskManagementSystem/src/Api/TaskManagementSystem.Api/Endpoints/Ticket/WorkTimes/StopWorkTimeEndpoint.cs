using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Api.Security;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.WorkTimes.StopWorkTime;

namespace TaskManagementSystem.Api.Endpoints.Ticket.WorkTimes;

/// <summary>POST /tickets/{id}/work-times/stop — stop work time on a ticket. Requires Tickets.Create permission.</summary>
public static class StopWorkTimeEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapPost("/{id:int}/work-times/stop", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Create);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int id,
        ICurrentUserAccessor currentUserAccessor,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var result = await mediator.Send(new StopWorkTimeCommand(id, userId), cancellationToken);
        return result.ToHttpResult(workTime => Results.Ok(TicketMapping.MapWorkTime(workTime)));
    }
}
