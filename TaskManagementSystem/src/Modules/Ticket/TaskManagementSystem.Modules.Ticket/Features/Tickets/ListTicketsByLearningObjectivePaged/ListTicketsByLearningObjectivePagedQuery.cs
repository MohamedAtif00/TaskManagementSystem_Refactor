using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Features;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsByLearningObjectivePaged;

public sealed record ListTicketsByLearningObjectivePagedQuery(
    int LearningObjectiveId,
    int? Page = null,
    int? PageSize = null) : IQuery<Result<TicketListPageResult>>;
