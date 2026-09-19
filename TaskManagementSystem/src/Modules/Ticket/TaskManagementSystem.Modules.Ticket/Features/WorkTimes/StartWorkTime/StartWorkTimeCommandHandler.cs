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
    : IRequestHandler<StartWorkTimeCommand, Result<TaskWorkTimeResult>>
{
    public async Task<Result<TaskWorkTimeResult>> Handle(
        StartWorkTimeCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.TicketTasks.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TaskWorkTimeResult>(TicketErrors.TicketNotFound);
        }

        if (!await identityUserLookup.ActiveUserExistsAsync(request.UserId, cancellationToken))
        {
            return Result.Fail<TaskWorkTimeResult>(TicketErrors.UserNotFound);
        }

        var existingOpen = await unitOfWork.TaskWorkTimes.GetOpenByTicketAndUserTrackedAsync(
            request.TicketId,
            request.UserId,
            cancellationToken);
        if (existingOpen is not null)
        {
            return Result.Fail<TaskWorkTimeResult>(TicketErrors.WorkTimeAlreadyOpen);
        }

        var createResult = Domain.TaskWorkTime.Start(request.TicketId, request.UserId, DateTime.UtcNow);
        if (!createResult.IsSuccess)
        {
            return Result.Fail<TaskWorkTimeResult>(createResult.Error);
        }

        await unitOfWork.TaskWorkTimes.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TaskWorkTimeResult.From(createResult.Value));
    }
}

