using MediatR;
using TaskManagementSystem.Api.Configuration;
using TaskManagementSystem.Api.Contracts.Ticket;
using TaskManagementSystem.Api.Infrastructure;
using TaskManagementSystem.Modules.Identity.Domain;
using TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketStats;

namespace TaskManagementSystem.Api.Endpoints.Ticket.Tickets;

public static class GetTicketStatsEndpoint
{
    public static RouteGroupBuilder Map(RouteGroupBuilder group)
    {
        group.MapGet("/stats", HandleAsync).RequirePermissionCode(PermissionCodes.Tickets.Read);
        return group;
    }

    private static async Task<IResult> HandleAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTicketStatsQuery(), cancellationToken);
        return result.ToHttpResult(stats =>
            Results.Ok(new TicketStatsResponse
            {
                Tickets = stats.Tickets
                    .Select(ticket => new TicketStatsTicketResponse
                    {
                        Id = ticket.Id,
                        Status = ticket.Status,
                        UserId = ticket.UserId,
                        LearningObjectiveId = ticket.LearningObjectiveId,
                        SubjectId = ticket.SubjectId
                    })
                    .ToList(),
                LearningObjectives = stats.LearningObjectives
                    .Select(lo => new TicketStatsLearningObjectiveResponse
                    {
                        Id = lo.Id,
                        SubjectId = lo.SubjectId
                    })
                    .ToList()
            }));
    }
}
