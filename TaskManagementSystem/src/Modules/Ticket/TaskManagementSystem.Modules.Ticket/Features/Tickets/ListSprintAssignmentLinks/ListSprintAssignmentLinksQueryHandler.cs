using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListSprintAssignmentLinks;

public sealed class ListSprintAssignmentLinksQueryHandler(SprintTicketsQueries sprintTicketsQueries)
    : IRequestHandler<ListSprintAssignmentLinksQuery, Result<IReadOnlyList<TicketAssignmentLinkResult>>>
{
    public async Task<Result<IReadOnlyList<TicketAssignmentLinkResult>>> Handle(
        ListSprintAssignmentLinksQuery request,
        CancellationToken cancellationToken)
    {
        var links = await sprintTicketsQueries.ListAssignmentLinksAsync(request.SprintId, cancellationToken);
        return Result.Ok(links);
    }
}
