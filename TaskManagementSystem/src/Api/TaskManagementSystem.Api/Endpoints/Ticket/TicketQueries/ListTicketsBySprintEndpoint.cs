using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySprint;

namespace TaskManagementSystem.Api.Endpoints.Ticket.TicketQueries;

/// <summary>GET /sprints/{sprintId}/tickets — list tickets by sprint. Requires Tickets.Read permission.</summary>
public static class ListTicketsBySprintEndpoint
{
    public static WebApplication Map(WebApplication app)
    {
        app.MapGet("/sprints/{sprintId:int}/tickets", HandleAsync)
            .WithTags("Tickets")
            .RequireAuthorization()
            .RequirePermissionCode(PermissionCodes.Tickets.Read);
        return app;
    }

    private static async Task<IResult> HandleAsync(
        int sprintId,
        int[]? status,
        int[]? learningObjectiveId,
        string? name,
        int[]? userId,
        bool? unassigned,
        int[]? priority,
        bool? flagged,
        bool? paused,
        bool? rolledBack,
        int? page,
        int? pageSize,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListTicketsBySprintQuery(
                sprintId,
                TicketListFilterBinder.Bind(
                    status,
                    learningObjectiveId,
                    name,
                    userId,
                    unassigned,
                    priority,
                    flagged,
                    paused,
                    rolledBack),
                page,
                pageSize),
            cancellationToken);
        return result.ToHttpResult(tickets => Results.Ok(TicketMapping.MapTicketListPage(tickets)));
    }
}
