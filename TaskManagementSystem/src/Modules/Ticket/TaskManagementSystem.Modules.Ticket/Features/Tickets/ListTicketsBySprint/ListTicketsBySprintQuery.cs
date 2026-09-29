using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySprint;

public sealed record ListTicketsBySprintQuery(
    int SprintId,
    TicketListFilter Filter,
    int? Page = null,
    int? PageSize = null) : IQuery<Result<TicketListPageResult>>;
