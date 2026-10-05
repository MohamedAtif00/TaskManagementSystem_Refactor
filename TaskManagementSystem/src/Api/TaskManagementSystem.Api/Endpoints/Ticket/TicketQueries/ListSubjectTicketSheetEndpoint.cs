using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListSubjectTicketSheet;

namespace TaskManagementSystem.Api.Endpoints.Ticket.TicketQueries;

/// <summary>GET /subjects/{subjectId}/tickets/sheet — every active ticket for a subject sheet. Requires Tickets.Read permission.</summary>
public static class ListSubjectTicketSheetEndpoint
{
    public static WebApplication Map(WebApplication app)
    {
        app.MapGet("/subjects/{subjectId:int}/tickets/sheet", HandleAsync)
            .WithTags("Tickets")
            .RequireAuthorization()
            .RequirePermissionCode(PermissionCodes.Tickets.Read);
        return app;
    }

    private static async Task<IResult> HandleAsync(
        int subjectId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListSubjectTicketSheetQuery(subjectId), cancellationToken);
        return result.ToHttpResult(items => Results.Ok(items.Select(Map).ToList()));
    }

    private static TicketSheetItemResponse Map(TicketSheetItemResult item) =>
        new()
        {
            Id = item.Id,
            Name = item.Name,
            Status = item.Status,
            LearningObjectiveId = item.LearningObjectiveId,
            UserId = item.UserId,
            UserName = item.UserName,
            Flagged = item.Flagged,
            Pause = item.Pause,
            IsRollback = item.IsRollback
        };
}
