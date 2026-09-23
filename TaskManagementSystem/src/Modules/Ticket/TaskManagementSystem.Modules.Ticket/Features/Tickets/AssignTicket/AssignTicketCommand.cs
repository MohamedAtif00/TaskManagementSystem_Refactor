using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.AssignTicket;

public sealed record AssignTicketCommand(int TicketId, int UserId, int ActorUserId, string ActorRole)
    : ITicketCommand<Result<TicketDetailResult>>;
