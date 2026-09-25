using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketStats;

public sealed record TicketStatsItemResult(
    int Id,
    int Status,
    int? UserId,
    int LearningObjectiveId,
    int SubjectId);

public sealed record TicketStatsLoResult(int Id, int SubjectId);

public sealed record TicketStatsResult(
    IReadOnlyList<TicketStatsItemResult> Tickets,
    IReadOnlyList<TicketStatsLoResult> LearningObjectives);

public sealed record GetTicketStatsQuery(
    IReadOnlyList<int>? SubjectIds = null,
    IReadOnlyList<int>? LearningObjectiveIds = null) : IQuery<Result<TicketStatsResult>>;
