using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketById;

public sealed class GetTicketByIdQueryHandler(ITicketUnitOfWork unitOfWork)
    : IRequestHandler<GetTicketByIdQuery, Result<TicketDetailResult>>
{
    public async Task<Result<TicketDetailResult>> Handle(
        GetTicketByIdQuery request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.TicketTasks.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketNotFound);
        }

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}

