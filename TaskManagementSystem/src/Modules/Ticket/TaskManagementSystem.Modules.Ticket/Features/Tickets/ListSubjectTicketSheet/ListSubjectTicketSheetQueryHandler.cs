using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListSubjectTicketSheet;

public sealed class ListSubjectTicketSheetQueryHandler(SubjectTicketsQueries subjectTicketsQueries)
    : IRequestHandler<ListSubjectTicketSheetQuery, Result<IReadOnlyList<TicketSheetItemResult>>>
{
    public async Task<Result<IReadOnlyList<TicketSheetItemResult>>> Handle(
        ListSubjectTicketSheetQuery request,
        CancellationToken cancellationToken)
    {
        var items = await subjectTicketsQueries.ListSheetBySubjectAsync(request.SubjectId, cancellationToken);
        return Result.Ok(items);
    }
}
