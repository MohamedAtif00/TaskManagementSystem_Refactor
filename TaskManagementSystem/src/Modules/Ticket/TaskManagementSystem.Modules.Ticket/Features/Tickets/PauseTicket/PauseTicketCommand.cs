using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.PauseTicket;

public sealed record PauseTicketCommand(int TicketId) : ITicketCommand<Result<TicketDetailResult>>;
