using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.WorkTimes.StopWorkTime;

public sealed record StopWorkTimeCommand(int TicketId, int UserId) : ITicketCommand<Result<TicketWorkTimeResult>>;

