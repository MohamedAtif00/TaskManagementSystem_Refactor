using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.CompleteTicket;

public sealed class CompleteTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    IWorkflowStepLookup workflowStepLookup,
    ITicketBankLookup ticketBankLookup,
    ITicketActivityWriter activityWriter,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<CompleteTicketCommand, Result<TicketDetailResult>>
{
    public async Task<Result<TicketDetailResult>> Handle(
        CompleteTicketCommand request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdTrackedAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketNotFound);
        }

        var allowOtherAssignee = TicketRoles.IsOwnerOrProjectManager(request.ActorRole);
        var completeResult = ticket.Complete(request.ActorUserId, allowOtherAssignee);
        if (!completeResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(completeResult.Error);
        }

        await TicketWorkClock.CloseOpenAsync(unitOfWork, ticket.Id, cancellationToken);
        await activityWriter.WriteAsync(
            ticket.Id,
            TicketActivityType.StatusDone,
            request.ActorUserId,
            cancellationToken);
        var opened = await TicketSuccessor.OpenNextAsync(
            unitOfWork,
            workflowStepLookup,
            ticketBankLookup,
            ticket,
            cancellationToken);
        var created = opened.Where(next => next.Id == 0).ToList();
        foreach (var next in opened.Where(next => next.Id != 0))
        {
            await activityWriter.WriteAsync(next.Id, TicketActivityType.Reactivated, actorOneId: null, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        foreach (var next in created)
        {
            await activityWriter.WriteAsync(next.Id, TicketActivityType.Created, actorOneId: null, cancellationToken);
        }

        if (created.Count > 0)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);
        foreach (var next in opened)
        {
            await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, next, cancellationToken);
        }

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}
