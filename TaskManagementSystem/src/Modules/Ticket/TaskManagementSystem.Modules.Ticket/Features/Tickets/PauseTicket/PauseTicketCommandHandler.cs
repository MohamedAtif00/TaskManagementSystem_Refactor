using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.PauseTicket;

public sealed class PauseTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    ITicketActivityWriter activityWriter,
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

        Result<NoValue> pauseResult;
        var openTimer = false;
        var activityType = ticket.Pause ? TicketActivityType.Resume : TicketActivityType.Pause;
        if (ticket.Pause)
        {
            pauseResult = ticket.Resume(request.ActorUserId);
            openTimer = pauseResult.IsSuccess && ticket.Status == TicketStatus.Doing;
        }
        else
        {
            pauseResult = ticket.PauseWork();
        }

        if (!pauseResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(pauseResult.Error);
        }

        if (openTimer)
        {
            await TicketWorkClock.OpenAsync(unitOfWork, ticket.Id, request.ActorUserId, cancellationToken);
        }
        else if (ticket.Pause)
        {
            await TicketWorkClock.CloseOpenAsync(unitOfWork, ticket.Id, cancellationToken);
        }

        await activityWriter.WriteAsync(ticket.Id, activityType, request.ActorUserId, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}
