using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.SkipTicket;

public sealed class SkipTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    IWorkflowStepLookup workflowStepLookup,
    ITicketBankLookup ticketBankLookup,
    ITicketActivityWriter activityWriter,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<SkipTicketCommand, Result<TicketDetailResult>>
{
    public async Task<Result<TicketDetailResult>> Handle(
        SkipTicketCommand request,
        CancellationToken cancellationToken)
    {
        if (!TicketRoles.IsOwnerOrProjectManager(request.ActorRole))
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketUnauthorized);
        }

        var ticket = await unitOfWork.Tickets.GetByIdTrackedAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketNotFound);
        }

        var skipResult = ticket.Skip();
        if (!skipResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(skipResult.Error);
        }

        await TicketWorkClock.CloseOpenAsync(unitOfWork, ticket.Id, cancellationToken);
        await activityWriter.WriteAsync(ticket.Id, TicketActivityType.Skip, request.ActorUserId, cancellationToken);

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
