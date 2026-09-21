using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using DomainTaskStatus = TaskManagementSystem.Modules.Ticket.Domain.TaskStatus;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySprint;

public sealed record ListTicketsBySprintQuery(
    int SprintId,
    IReadOnlyList<DomainTaskStatus>? Statuses = null,
    int? LearningObjectiveId = null,
    string? Name = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Result<TicketListPageResult>>;
