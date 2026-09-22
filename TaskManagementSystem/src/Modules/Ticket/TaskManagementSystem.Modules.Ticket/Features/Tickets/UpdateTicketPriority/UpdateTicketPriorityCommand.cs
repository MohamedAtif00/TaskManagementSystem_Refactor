using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.UpdateTicketPriority;

public sealed record UpdateTicketPriorityCommand(int TicketId, TicketPriority Priority, int ActorUserId)
    : ITicketCommand<Result<TicketDetailResult>>;
