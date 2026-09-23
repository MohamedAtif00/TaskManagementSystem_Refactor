using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.CompleteTicket;

public sealed record CompleteTicketCommand(int TicketId, int ActorUserId, string ActorRole)
    : ITicketCommand<Result<TicketDetailResult>>;
