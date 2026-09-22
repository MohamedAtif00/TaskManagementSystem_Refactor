using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.GetJumpPoints;

public sealed record GetJumpPointsQuery(int TicketId) : ITicketCommand<Result<IReadOnlyList<JumpPointResult>>>;
