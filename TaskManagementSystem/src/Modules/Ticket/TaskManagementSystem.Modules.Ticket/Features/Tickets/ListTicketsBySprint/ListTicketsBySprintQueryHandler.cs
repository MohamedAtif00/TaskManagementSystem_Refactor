using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySprint;

public sealed class ListTicketsBySprintQueryHandler(SprintTicketsQueries sprintTicketsQueries)
    : IRequestHandler<ListTicketsBySprintQuery, Result<TicketListPageResult>>
{
    public async Task<Result<TicketListPageResult>> Handle(
        ListTicketsBySprintQuery request,
        CancellationToken cancellationToken)
    {
        var tickets = await sprintTicketsQueries.ListBySprintAsync(
            request.SprintId,
            request.Statuses,
            request.LearningObjectiveId,
            request.Name,
            request.Page,
            request.PageSize,
            cancellationToken);
        return Result.Ok(tickets);
    }
}
