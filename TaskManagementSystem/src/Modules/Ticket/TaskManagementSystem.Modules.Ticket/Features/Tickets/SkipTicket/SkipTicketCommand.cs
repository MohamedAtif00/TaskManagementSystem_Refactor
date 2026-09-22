using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.SkipTicket;

public sealed record SkipTicketCommand(int TicketId, int ActorUserId) : ITicketCommand<Result<TicketDetailResult>>;
