using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketSummary;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

public static class GetTicketSummaryEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/summary", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        int? subjectId,
        int? sprintId,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTicketSummaryQuery(subjectId, sprintId), cancellationToken);
        return result.ToHttpResult(summary =>
            Results.Ok(new TicketSummaryResponse
            {
                Backlog = summary.Backlog,
                ToDo = summary.ToDo,
                Doing = summary.Doing,
                Done = summary.Done,
                TotalCount = summary.TotalCount,
                CalculatedAtUtc = summary.CalculatedAtUtc
            }));
    }
}
