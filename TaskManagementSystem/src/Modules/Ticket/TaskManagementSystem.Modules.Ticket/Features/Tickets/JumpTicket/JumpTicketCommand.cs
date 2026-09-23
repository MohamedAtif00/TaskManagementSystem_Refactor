using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.JumpTicket;

public sealed record JumpTicketCommand(int TicketId, int StepId, int ActorUserId, string ActorRole)
    : ITicketCommand<Result<TicketDetailResult>>;
