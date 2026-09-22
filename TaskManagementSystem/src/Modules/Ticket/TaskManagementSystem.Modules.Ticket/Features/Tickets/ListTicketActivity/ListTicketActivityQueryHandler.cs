using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketActivity;

public sealed class ListTicketActivityQueryHandler(ITicketUnitOfWork unitOfWork)
    : IRequestHandler<ListTicketActivityQuery, Result<IReadOnlyList<TicketActivityListItemResult>>>
{
    public async Task<Result<IReadOnlyList<TicketActivityListItemResult>>> Handle(
        ListTicketActivityQuery request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<IReadOnlyList<TicketActivityListItemResult>>(TicketErrors.TicketNotFound);
        }

        var rows = await unitOfWork.TaskActivities.ListByTicketAsync(request.TicketId, cancellationToken);
        return Result.Ok<IReadOnlyList<TicketActivityListItemResult>>(
            rows.Select(TicketActivityListItemResult.From).ToList());
    }
}
