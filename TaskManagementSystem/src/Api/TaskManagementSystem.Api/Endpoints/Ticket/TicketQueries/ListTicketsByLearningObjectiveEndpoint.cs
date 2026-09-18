using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsByLearningObjective;

namespace TaskManagementSystem.Api.Endpoints.Ticket.TicketQueries;

/// <summary>GET /learning-objectives/{loId}/tickets — list tickets by learning objective. Requires Tickets.Read permission.</summary>
public static class ListTicketsByLearningObjectiveEndpoint
{
    public static WebApplication Map(WebApplication app)
    {
        app.MapGet("/learning-objectives/{loId:int}/tickets", HandleAsync)
            .WithTags("Tickets")
            .RequireAuthorization()
            .RequirePermissionCode(PermissionCodes.Tickets.Read);
        return app;
    }

    private static async Task<IResult> HandleAsync(
        int loId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ListTicketsByLearningObjectiveQuery(loId), cancellationToken);
        return result.ToHttpResult(tickets =>
            Results.Ok(tickets.Select(TicketMapping.MapTicketListItem).ToList()));
    }
}
