using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsByLearningObjective;

public sealed class ListTicketsByLearningObjectiveQueryHandler(ITicketUnitOfWork unitOfWork)
    : IRequestHandler<ListTicketsByLearningObjectiveQuery, Result<IReadOnlyList<TicketListItemResult>>>
{
    public async Task<Result<IReadOnlyList<TicketListItemResult>>> Handle(
        ListTicketsByLearningObjectiveQuery request,
        CancellationToken cancellationToken)
    {
        var tickets = await unitOfWork.Tickets.ListByLearningObjectiveAsync(
            request.LearningObjectiveId,
            cancellationToken);

        return Result.Ok<IReadOnlyList<TicketListItemResult>>(tickets.Select(TicketListItemResult.From).ToList());
    }
}

