using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.WorkTimes.StopWorkTime;

public sealed class StopWorkTimeCommandHandler(
    ITicketUnitOfWork unitOfWork,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<StopWorkTimeCommand, Result<TicketWorkTimeResult>>
{
    public async Task<Result<TicketWorkTimeResult>> Handle(
        StopWorkTimeCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketWorkTimeResult>(TicketErrors.TicketNotFound);
        }

        var workTime = await unitOfWork.TicketWorkTimes.GetOpenByTicketAndUserTrackedAsync(
            request.TicketId,
            request.UserId,
            cancellationToken);
        if (workTime is null)
        {
            return Result.Fail<TicketWorkTimeResult>(TicketErrors.WorkTimeNotFound);
        }

        var stopResult = workTime.Stop(DateTime.UtcNow);
        if (!stopResult.IsSuccess)
        {
            return Result.Fail<TicketWorkTimeResult>(stopResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TicketWorkTimeResult.From(workTime));
    }
}

