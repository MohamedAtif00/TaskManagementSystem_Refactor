using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.RollbackTicket;

public sealed record RollbackTicketCommand(int TicketId, int ActorUserId, string ActorRole)
    : ITicketCommand<Result<TicketDetailResult>>;
