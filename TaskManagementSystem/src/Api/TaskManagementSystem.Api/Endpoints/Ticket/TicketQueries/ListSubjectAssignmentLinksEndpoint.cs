using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListSubjectAssignmentLinks;

namespace TaskManagementSystem.Api.Endpoints.Ticket.TicketQueries;

/// <summary>GET /subjects/{subjectId}/tickets/assignments — distinct assignee and learning-objective pairs. Requires Tickets.Read permission.</summary>
public static class ListSubjectAssignmentLinksEndpoint
{
    public static WebApplication Map(WebApplication app)
    {
        app.MapGet("/subjects/{subjectId:int}/tickets/assignments", HandleAsync)
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
        var result = await mediator.Send(new ListSubjectAssignmentLinksQuery(subjectId), cancellationToken);
        return result.ToHttpResult(links => Results.Ok(links));
    }
}
