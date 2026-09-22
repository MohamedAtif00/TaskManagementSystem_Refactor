using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using DomainTicketStatus = TaskManagementSystem.Modules.Ticket.Domain.TicketStatus;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySprint;

public sealed record ListTicketsBySprintQuery(
    int SprintId,
    IReadOnlyList<DomainTicketStatus>? Statuses = null,
    int? LearningObjectiveId = null,
    string? Name = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Result<TicketListPageResult>>;
