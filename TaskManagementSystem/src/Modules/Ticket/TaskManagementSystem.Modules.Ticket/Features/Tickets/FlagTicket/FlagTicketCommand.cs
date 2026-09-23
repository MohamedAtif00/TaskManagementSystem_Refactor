using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.FlagTicket;

public sealed record FlagTicketCommand(int TicketId, int ActorUserId) : ITicketCommand<Result<TicketDetailResult>>;
