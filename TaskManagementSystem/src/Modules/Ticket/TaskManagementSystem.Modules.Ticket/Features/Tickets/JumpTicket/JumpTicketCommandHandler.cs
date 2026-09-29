using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.JumpTicket;

public sealed class JumpTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    ILearningObjectiveLookup learningObjectiveLookup,
    IWorkflowStepLookup workflowStepLookup,
    ITicketBankLookup ticketBankLookup,
    ITicketActivityWriter activityWriter,
    IRealtimePublisher realtimePublisher)
    : IRequestHandler<JumpTicketCommand, Result<TicketDetailResult>>
{
    public async Task<Result<TicketDetailResult>> Handle(
        JumpTicketCommand request,
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

        if (ticket.StepId is not int currentStepId)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.StepNotFound);
        }

        var learningObjective = await learningObjectiveLookup.GetActiveByIdAsync(
            ticket.LearningObjectiveId,
            cancellationToken);
        if (learningObjective is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.LearningObjectiveNotFound);
        }

        var destinationIds = request.StepIds.Where(stepId => stepId > 0).Distinct().ToArray();
        if (destinationIds.Length == 0)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.StepNotFound);
        }

        foreach (var stepId in destinationIds)
        {
            if (!await workflowStepLookup.IsStepAheadAsync(
                    learningObjective.SchemaId,
                    currentStepId,
                    stepId,
                    cancellationToken))
            {
                return Result.Fail<TicketDetailResult>(TicketErrors.StepNotFound);
            }
        }

        var intervening = new HashSet<int>();
        foreach (var stepId in destinationIds)
        {
            var between = await workflowStepLookup.ListStepIdsBetweenAsync(
                learningObjective.SchemaId,
                currentStepId,
                stepId,
                cancellationToken);
            foreach (var betweenId in between)
            {
                intervening.Add(betweenId);
            }
        }

        intervening.Remove(currentStepId);
        foreach (var stepId in destinationIds)
        {
            intervening.Remove(stepId);
        }

        var bypassed = ticket.MarkBypassed();
        if (!bypassed.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(bypassed.Error);
        }

        await TicketWorkClock.CloseOpenAsync(unitOfWork, ticket.Id, cancellationToken);

        foreach (var stepId in intervening)
        {
            var work = await unitOfWork.Tickets.ListActiveByStepTrackedAsync(
                stepId,
                ticket.LearningObjectiveId,
                cancellationToken);
            foreach (var other in work)
            {
                if (other.Id == ticket.Id)
                {
                    continue;
                }

                var marked = other.MarkBypassed();
                if (!marked.IsSuccess)
                {
                    return Result.Fail<TicketDetailResult>(marked.Error);
                }

                await TicketWorkClock.CloseOpenAsync(unitOfWork, other.Id, cancellationToken);
            }
        }

        var opened = new List<Domain.Ticket>();
        foreach (var stepId in destinationIds)
        {
            var step = await workflowStepLookup.GetActiveByIdAsync(stepId, cancellationToken);
            if (step is null)
            {
                return Result.Fail<TicketDetailResult>(TicketErrors.StepNotFound);
            }

            var destinations = await TicketSuccessor.OpenDestinationAsync(
                unitOfWork,
                ticketBankLookup,
                ticket.LearningObjectiveId,
                step,
                cancellationToken);
            if (destinations.Count == 0)
            {
                return Result.Fail<TicketDetailResult>(TicketErrors.TicketBankNotFound);
            }

            opened.AddRange(destinations);
        }

        foreach (var destination in opened)
        {
            if (destination.Id > 0)
            {
                await TicketWorkClock.CloseOpenAsync(unitOfWork, destination.Id, cancellationToken);
            }
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await activityWriter.WriteAsync(
            ticket.Id,
            TicketActivityType.Jump,
            request.ActorUserId,
            cancellationToken,
            additionalInfo: string.Join(", ", destinationIds));
        foreach (var destination in opened)
        {
            await activityWriter.WriteAsync(
                destination.Id,
                TicketActivityType.ReactivateJump,
                request.ActorUserId,
                cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);
        foreach (var destination in opened)
        {
            await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, destination, cancellationToken);
        }

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}
