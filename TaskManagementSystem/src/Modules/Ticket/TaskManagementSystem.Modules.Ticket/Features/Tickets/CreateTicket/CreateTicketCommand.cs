using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.CreateTicket;

public sealed record CreateTicketCommand(
    int LearningObjectiveId,
    int TicketBankItemId,
    int? UserId) : ITicketCommand<Result<TicketDetailResult>>;

