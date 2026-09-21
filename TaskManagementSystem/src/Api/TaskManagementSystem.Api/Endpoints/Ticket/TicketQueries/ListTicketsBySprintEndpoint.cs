using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySprint;
using DomainTaskStatus = TaskManagementSystem.Modules.Ticket.Domain.TaskStatus;

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
        int? learningObjectiveId,
        string? name,
        int? page,
        int? pageSize,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var statuses = status is { Length: > 0 }
            ? status.Select(value => (DomainTaskStatus)value).ToArray()
            : null;
        var result = await mediator.Send(
            new ListTicketsBySprintQuery(
                sprintId,
                statuses,
                learningObjectiveId,
                name,
                page,
                page is null ? null : pageSize ?? 20),
            cancellationToken);
        return result.ToHttpResult(tickets =>
            page is null
                ? Results.Ok(tickets.Items.Select(TicketMapping.MapTicketListItem).ToList())
                : Results.Ok(TicketMapping.MapTicketListPage(tickets)));
    }
}
