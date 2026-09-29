using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListSubjectAssignmentLinks;

public sealed class ListSubjectAssignmentLinksQueryHandler(SubjectTicketsQueries subjectTicketsQueries)
    : IRequestHandler<ListSubjectAssignmentLinksQuery, Result<IReadOnlyList<TicketAssignmentLinkResult>>>
{
    public async Task<Result<IReadOnlyList<TicketAssignmentLinkResult>>> Handle(
        ListSubjectAssignmentLinksQuery request,
        CancellationToken cancellationToken)
    {
        var links = await subjectTicketsQueries.ListAssignmentLinksAsync(request.SubjectId, cancellationToken);
        return Result.Ok(links);
    }
}
