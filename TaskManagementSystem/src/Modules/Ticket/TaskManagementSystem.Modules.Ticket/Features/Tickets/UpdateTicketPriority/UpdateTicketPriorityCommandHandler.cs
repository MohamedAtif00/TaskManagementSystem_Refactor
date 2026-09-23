using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.UpdateTicketPriority;

public sealed class UpdateTicketPriorityCommandHandler(
    ITicketUnitOfWork unitOfWork,
    ITicketActivityWriter activityWriter,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<UpdateTicketPriorityCommand, Result<TicketDetailResult>>
{
    public async Task<Result<TicketDetailResult>> Handle(
        UpdateTicketPriorityCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdTrackedAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketNotFound);
        }

        var updateResult = ticket.ChangePriority(request.Priority);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(updateResult.Error);
        }

        await activityWriter.WriteAsync(
            ticket.Id,
            TicketActivityType.PriorityChange,
            request.ActorUserId,
            cancellationToken,
            additionalInfo: request.Priority.ToString());

        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}
