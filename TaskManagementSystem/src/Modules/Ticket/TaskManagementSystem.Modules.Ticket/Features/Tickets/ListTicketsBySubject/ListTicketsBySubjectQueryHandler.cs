using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySubject;

public sealed class ListTicketsBySubjectQueryHandler(SubjectTicketsQueries subjectTicketsQueries)
    : IRequestHandler<ListTicketsBySubjectQuery, Result<TicketListPageResult>>
{
    public async Task<Result<TicketListPageResult>> Handle(
        ListTicketsBySubjectQuery request,
        CancellationToken cancellationToken)
    {
        return await subjectTicketsQueries.ListBySubjectAsync(
            request.SubjectId,
            request.Statuses,
            request.LearningObjectiveId,
            request.Name,
            request.Page,
            request.PageSize,
            cancellationToken);
    }
}
