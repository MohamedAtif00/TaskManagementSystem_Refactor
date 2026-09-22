using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySubject;
using DomainTicketStatus = TaskManagementSystem.Modules.Ticket.Domain.TicketStatus;

namespace TaskManagementSystem.Api.Endpoints.Ticket.TicketQueries;

/// <summary>GET /subjects/{subjectId}/tickets — list tickets by subject. Requires Tickets.Read permission.</summary>
public static class ListTicketsBySubjectEndpoint
{
    public static WebApplication Map(WebApplication app)
    {
        app.MapGet("/subjects/{subjectId:int}/tickets", HandleAsync)
            .WithTags("Tickets")
            .RequireAuthorization()
            .RequirePermissionCode(PermissionCodes.Tickets.Read);
        return app;
    }

    private static async Task<IResult> HandleAsync(
        int subjectId,
        int[]? status,
        int? learningObjectiveId,
        string? name,
        int? page,
        int? pageSize,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var statuses = status is { Length: > 0 }
            ? status.Select(value => (DomainTicketStatus)value).ToArray()
            : null;
        var result = await mediator.Send(
            new ListTicketsBySubjectQuery(
                subjectId,
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
