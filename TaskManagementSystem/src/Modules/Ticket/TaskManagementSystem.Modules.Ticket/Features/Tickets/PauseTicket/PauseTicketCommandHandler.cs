using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.PauseTicket;

public sealed class PauseTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<PauseTicketCommand, Result<TicketDetailResult>>
{
    public async Task<Result<TicketDetailResult>> Handle(
        PauseTicketCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdTrackedAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketNotFound);
        }

        var pauseResult = ticket.TogglePause();
        if (!pauseResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(pauseResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}
