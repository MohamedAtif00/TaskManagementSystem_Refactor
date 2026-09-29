using DomainTicketPriority = TaskManagementSystem.Modules.Ticket.Domain.TicketPriority;
using DomainTicketStatus = TaskManagementSystem.Modules.Ticket.Domain.TicketStatus;

namespace TaskManagementSystem.Modules.Ticket.Features;

public sealed record TicketListFilter(
    IReadOnlyList<DomainTicketStatus>? Statuses = null,
    IReadOnlyList<int>? LearningObjectiveIds = null,
    string? Name = null,
    IReadOnlyList<int>? UserIds = null,
    bool Unassigned = false,
    IReadOnlyList<DomainTicketPriority>? Priorities = null,
    bool Flagged = false,
    bool Paused = false,
    bool RolledBack = false);
