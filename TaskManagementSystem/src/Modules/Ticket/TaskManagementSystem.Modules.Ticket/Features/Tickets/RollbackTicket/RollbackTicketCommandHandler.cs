using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.RollbackTicket;

public sealed class RollbackTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    ITicketActivityWriter activityWriter,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<RollbackTicketCommand, Result<TicketDetailResult>>
{
    public async Task<Result<TicketDetailResult>> Handle(
        RollbackTicketCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdTrackedAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketNotFound);
        }

        var allowOtherAssignee = TicketRoles.IsOwnerOrProjectManager(request.ActorRole);
        var rollbackResult = ticket.Rollback(request.ActorUserId, allowOtherAssignee);
        if (!rollbackResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(rollbackResult.Error);
        }

        await TicketWorkClock.CloseOpenAsync(unitOfWork, ticket.Id, cancellationToken);
        await activityWriter.WriteAsync(
            ticket.Id,
            TicketActivityType.StatusRollback,
            request.ActorUserId,
            cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}
