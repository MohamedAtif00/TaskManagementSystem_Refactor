using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySubject;

public sealed class ListTicketsBySubjectQueryHandler(SubjectTicketsQueries subjectTicketsQueries)
    : IRequestHandler<ListTicketsBySubjectQuery, Result<IReadOnlyList<TicketListItemResult>>>
{
    public async Task<Result<IReadOnlyList<TicketListItemResult>>> Handle(
        ListTicketsBySubjectQuery request,
        CancellationToken cancellationToken)
    {
        var tickets = await subjectTicketsQueries.ListBySubjectAsync(request.SubjectId, cancellationToken);
        return Result.Ok<IReadOnlyList<TicketListItemResult>>(tickets);
    }
}

