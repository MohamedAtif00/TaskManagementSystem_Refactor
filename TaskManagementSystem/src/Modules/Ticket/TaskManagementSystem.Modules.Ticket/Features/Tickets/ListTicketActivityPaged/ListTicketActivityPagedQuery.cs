using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketActivityPaged;

public sealed record ListTicketActivityPagedQuery(
    int TicketId,
    int? Page = null,
    int? PageSize = null) : IQuery<Result<TicketActivityListPageResult>>;
