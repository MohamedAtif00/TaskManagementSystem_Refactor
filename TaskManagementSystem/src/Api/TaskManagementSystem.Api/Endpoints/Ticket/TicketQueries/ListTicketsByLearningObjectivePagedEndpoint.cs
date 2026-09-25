using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsByLearningObjectivePaged;

namespace TaskManagementSystem.Api.Endpoints.Ticket.TicketQueries;

/// <summary>GET /learning-objectives/{loId}/tickets/paged — paged tickets by learning objective.</summary>
public static class ListTicketsByLearningObjectivePagedEndpoint
{
    public static WebApplication Map(WebApplication app)
    {
        app.MapGet("/learning-objectives/{loId:int}/tickets/paged", HandleAsync)
            .WithTags("Tickets")
            .RequireAuthorization()
            .RequirePermissionCode(PermissionCodes.Tickets.Read);
        return app;
    }

    private static async Task<IResult> HandleAsync(
        int loId,
        int? page,
        int? pageSize,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ListTicketsByLearningObjectivePagedQuery(loId, page, pageSize),
            cancellationToken);
        return result.ToHttpResult(tickets => Results.Ok(TicketMapping.MapTicketListPage(tickets)));
    }
}
