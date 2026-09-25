using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketSummary;

public sealed record TicketSummaryResult(
    int Backlog,
    int ToDo,
    int Doing,
    int Done,
    int TotalCount,
    DateTime CalculatedAtUtc);

public sealed record GetTicketSummaryQuery(int? SubjectId, int? SprintId) : IQuery<Result<TicketSummaryResult>>;
