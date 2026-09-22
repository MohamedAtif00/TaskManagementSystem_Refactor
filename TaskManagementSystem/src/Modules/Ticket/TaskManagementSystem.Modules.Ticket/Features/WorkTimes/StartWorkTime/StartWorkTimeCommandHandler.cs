using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.WorkTimes.StartWorkTime;

public sealed class StartWorkTimeCommandHandler(
    ITicketUnitOfWork unitOfWork,
    IIdentityUserLookup identityUserLookup,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<StartWorkTimeCommand, Result<TicketWorkTimeResult>>
{
    public async Task<Result<TicketWorkTimeResult>> Handle(
        StartWorkTimeCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketWorkTimeResult>(TicketErrors.TicketNotFound);
        }

        if (!await identityUserLookup.ActiveUserExistsAsync(request.UserId, cancellationToken))
        {
            return Result.Fail<TicketWorkTimeResult>(TicketErrors.UserNotFound);
        }

        var existingOpen = await unitOfWork.TicketWorkTimes.GetOpenByTicketAndUserTrackedAsync(
            request.TicketId,
            request.UserId,
            cancellationToken);
        if (existingOpen is not null)
        {
            return Result.Fail<TicketWorkTimeResult>(TicketErrors.WorkTimeAlreadyOpen);
        }

        var createResult = Domain.TicketWorkTime.Start(request.TicketId, request.UserId, DateTime.UtcNow);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<TicketWorkTimeResult>(createResult.Error);
        }

        await unitOfWork.TicketWorkTimes.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TicketWorkTimeResult.From(createResult.Value));
    }
}

