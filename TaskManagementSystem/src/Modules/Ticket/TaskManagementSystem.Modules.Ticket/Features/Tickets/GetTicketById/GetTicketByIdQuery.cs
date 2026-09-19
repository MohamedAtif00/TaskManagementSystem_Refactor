using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketById;

public sealed record GetTicketByIdQuery(int TicketId) : IQuery<Result<TicketDetailResult>>;

