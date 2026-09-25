using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsByLearningObjectivePaged;

public sealed class ListTicketsByLearningObjectivePagedQueryHandler(
    LearningObjectiveTicketsQueries learningObjectiveTicketsQueries)
    : IRequestHandler<ListTicketsByLearningObjectivePagedQuery, Result<TicketListPageResult>>
{
    public Task<Result<TicketListPageResult>> Handle(
        ListTicketsByLearningObjectivePagedQuery request,
        CancellationToken cancellationToken) =>
        learningObjectiveTicketsQueries.ListByLearningObjectiveAsync(
            request.LearningObjectiveId,
            request.Page,
            request.PageSize,
            cancellationToken);
}
