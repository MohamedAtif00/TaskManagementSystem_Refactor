using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;
using DomainTaskStatus = TaskManagementSystem.Modules.Ticket.Domain.TaskStatus;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySubject;

public sealed record ListTicketsBySubjectQuery(
    int SubjectId,
    IReadOnlyList<DomainTaskStatus>? Statuses = null,
    int? LearningObjectiveId = null,
    string? Name = null,
    int? Page = null,
    int? PageSize = null) : IQuery<Result<TicketListPageResult>>;
