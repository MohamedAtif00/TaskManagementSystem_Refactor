using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.GetRollbackPoints;

public sealed record GetRollbackPointsQuery(int TicketId) : ITicketCommand<Result<IReadOnlyList<JumpPointResult>>>;
