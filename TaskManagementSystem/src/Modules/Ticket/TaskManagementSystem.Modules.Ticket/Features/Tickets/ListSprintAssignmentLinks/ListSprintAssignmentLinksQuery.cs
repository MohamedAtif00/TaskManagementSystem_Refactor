using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListSprintAssignmentLinks;

public sealed record ListSprintAssignmentLinksQuery(int SprintId)
    : IQuery<Result<IReadOnlyList<TicketAssignmentLinkResult>>>;
