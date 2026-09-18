using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.WorkTimes.StopWorkTime;

public sealed record StopWorkTimeCommand(int TicketId, int UserId) : ITicketCommand<Result<TaskWorkTimeResult>>;

public sealed class StopWorkTimeCommandValidator : AbstractValidator<StopWorkTimeCommand>
{
    public StopWorkTimeCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

public sealed class StopWorkTimeCommandHandler(
    ITicketUnitOfWork unitOfWork,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<StopWorkTimeCommand, Result<TaskWorkTimeResult>>
{
    public async Task<Result<TaskWorkTimeResult>> Handle(
        StopWorkTimeCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.TicketTasks.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TaskWorkTimeResult>(TicketErrors.TicketNotFound);
        }

        var workTime = await unitOfWork.TaskWorkTimes.GetOpenByTicketAndUserTrackedAsync(
            request.TicketId,
            request.UserId,
            cancellationToken);
        if (workTime is null)
        {
            return Result.Fail<TaskWorkTimeResult>(TicketErrors.WorkTimeNotFound);
        }

        var stopResult = workTime.Stop(DateTime.UtcNow);
        if (!stopResult.IsSuccess)
        {
            return Result.Fail<TaskWorkTimeResult>(stopResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TaskWorkTimeResult.From(workTime));
    }
}
