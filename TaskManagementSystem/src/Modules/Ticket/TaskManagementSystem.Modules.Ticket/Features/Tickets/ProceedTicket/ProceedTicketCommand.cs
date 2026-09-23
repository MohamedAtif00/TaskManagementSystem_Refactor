using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ProceedTicket;

public sealed record ProceedTicketCommand(int TicketId, int ActorUserId) : ITicketCommand<Result<TicketDetailResult>>;
