using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListSprintAssignmentLinks;

namespace TaskManagementSystem.Api.Endpoints.Ticket.TicketQueries;

/// <summary>GET /sprints/{sprintId}/tickets/assignments — distinct assignee and learning-objective pairs. Requires Tickets.Read permission.</summary>
public static class ListSprintAssignmentLinksEndpoint
{
    public static WebApplication Map(WebApplication app)
    {
        app.MapGet("/sprints/{sprintId:int}/tickets/assignments", HandleAsync)
            .WithTags("Tickets")
            .RequireAuthorization()
            .RequirePermissionCode(PermissionCodes.Tickets.Read);
        return app;
    }

    private static async Task<IResult> HandleAsync(
        int sprintId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSprintAssignmentLinksQuery(sprintId), cancellationToken);
        return result.ToHttpResult(links => Results.Ok(links));
    }
}
