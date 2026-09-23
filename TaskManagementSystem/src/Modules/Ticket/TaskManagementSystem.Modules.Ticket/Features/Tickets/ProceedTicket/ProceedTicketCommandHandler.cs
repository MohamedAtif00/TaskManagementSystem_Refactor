using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ProceedTicket;

public sealed class ProceedTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    ITicketActivityWriter activityWriter,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<ProceedTicketCommand, Result<TicketDetailResult>>
{
    public async Task<Result<TicketDetailResult>> Handle(
        ProceedTicketCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdTrackedAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketNotFound);
        }

        Result<NoValue> moveResult;
        var openTimer = false;
        TicketActivityType activityType;
        if (ticket.Status == TicketStatus.Backlog)
        {
            activityType = TicketActivityType.StatusToDo;
            moveResult = ticket.Add(request.ActorUserId);
        }
        else if (ticket.Status == TicketStatus.ToDo && ticket.Pause)
        {
            activityType = TicketActivityType.Resume;
            moveResult = ticket.Resume(request.ActorUserId);
            openTimer = moveResult.IsSuccess && ticket.Status == TicketStatus.Doing;
        }
        else if (ticket.Status == TicketStatus.ToDo)
        {
            activityType = TicketActivityType.StatusDoing;
            moveResult = ticket.Start(request.ActorUserId);
            openTimer = moveResult.IsSuccess;
        }
        else
        {
            return Result.Fail<TicketDetailResult>(
                new ResultError("ticket_cannot_proceed", "Task cannot proceed from this column."));
        }

        if (!moveResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(moveResult.Error);
        }

        if (openTimer)
        {
            await TicketWorkClock.OpenAsync(unitOfWork, ticket.Id, request.ActorUserId, cancellationToken);
        }

        await activityWriter.WriteAsync(ticket.Id, activityType, request.ActorUserId, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}
