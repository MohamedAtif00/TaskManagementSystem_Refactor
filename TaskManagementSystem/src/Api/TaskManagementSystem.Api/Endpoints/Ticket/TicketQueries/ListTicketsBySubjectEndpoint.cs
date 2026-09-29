using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySubject;
using DomainTicketPriority = TaskManagementSystem.Modules.Ticket.Domain.TicketPriority;
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
            new ListTicketsBySubjectQuery(
                subjectId,
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

internal static class TicketListFilterBinder
{
    public static TicketListFilter Bind(
        int[]? status,
        int[]? learningObjectiveId,
        string? name,
        int[]? userId,
        bool? unassigned,
        int[]? priority,
        bool? flagged,
        bool? paused,
        bool? rolledBack) =>
        new(
            status is { Length: > 0 } ? status.Select(value => (DomainTicketStatus)value).ToArray() : null,
            learningObjectiveId is { Length: > 0 } ? learningObjectiveId : null,
            name,
            userId is { Length: > 0 } ? userId : null,
            unassigned == true,
            priority is { Length: > 0 } ? priority.Select(value => (DomainTicketPriority)value).ToArray() : null,
            flagged == true,
            paused == true,
            rolledBack == true);
}
