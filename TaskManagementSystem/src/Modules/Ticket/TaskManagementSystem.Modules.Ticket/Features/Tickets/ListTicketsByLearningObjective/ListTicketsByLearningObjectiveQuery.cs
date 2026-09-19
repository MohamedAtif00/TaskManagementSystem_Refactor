using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsByLearningObjective;

public sealed record ListTicketsByLearningObjectiveQuery(int LearningObjectiveId)
    : IQuery<Result<IReadOnlyList<TicketListItemResult>>>;

