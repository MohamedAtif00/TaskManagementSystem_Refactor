using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketActivity;

public sealed record ListTicketActivityQuery(int TicketId) : ITicketCommand<Result<IReadOnlyList<TicketActivityListItemResult>>>;
