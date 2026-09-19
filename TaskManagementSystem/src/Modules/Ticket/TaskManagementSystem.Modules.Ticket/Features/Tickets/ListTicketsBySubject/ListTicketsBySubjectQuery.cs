using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySubject;

public sealed record ListTicketsBySubjectQuery(int SubjectId)
    : IQuery<Result<IReadOnlyList<TicketListItemResult>>>;

