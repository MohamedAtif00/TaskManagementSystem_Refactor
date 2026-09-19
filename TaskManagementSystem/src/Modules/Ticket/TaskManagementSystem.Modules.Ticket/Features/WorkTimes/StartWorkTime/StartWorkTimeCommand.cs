using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.WorkTimes.StartWorkTime;

public sealed record StartWorkTimeCommand(int TicketId, int UserId) : ITicketCommand<Result<TaskWorkTimeResult>>;

