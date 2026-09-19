using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySprint;

public sealed record ListTicketsBySprintQuery(int SprintId)
    : IQuery<Result<IReadOnlyList<TicketListItemResult>>>;

