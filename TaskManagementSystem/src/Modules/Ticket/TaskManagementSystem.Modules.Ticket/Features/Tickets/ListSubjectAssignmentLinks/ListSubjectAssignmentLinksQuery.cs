using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListSubjectAssignmentLinks;

public sealed record ListSubjectAssignmentLinksQuery(int SubjectId)
    : IQuery<Result<IReadOnlyList<TicketAssignmentLinkResult>>>;
