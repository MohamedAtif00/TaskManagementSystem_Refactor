using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;
using TaskManagementSystem.Modules.Ticket.Domain;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.RollbackTicket;

public sealed class RollbackTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    ILearningObjectiveLookup learningObjectiveLookup,
    IWorkflowStepLookup workflowStepLookup,
    ITicketBankLookup ticketBankLookup,
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

        var earlierSteps = await workflowStepLookup.ListStepsBehindAsync(
            learningObjective.SchemaId,
            currentStepId,
            cancellationToken);
        if (earlierSteps.All(step => step.StepId != request.StepId))
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.StepNotFound);
        }

        var allowOtherAssignee = TicketRoles.IsOwnerOrProjectManager(request.ActorRole);
        var rollbackResult = ticket.Rollback(request.ActorUserId, allowOtherAssignee);
        if (!rollbackResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(rollbackResult.Error);
        }

        var correction = await TicketSuccessor.OpenCorrectionAsync(
            unitOfWork,
            ticketBankLookup,
            workflowStepLookup,
            ticket,
            request.StepId,
            cancellationToken);
        if (correction is null)
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.TicketBankNotFound);
        }

        await TicketWorkClock.CloseOpenAsync(unitOfWork, ticket.Id, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var notes = $"Clarification: {request.Clarification.Trim()} Issue notes: {request.IssueNotes.Trim()}";
        await activityWriter.WriteAsync(
            ticket.Id,
            TicketActivityType.StatusRollback,
            request.ActorUserId,
            cancellationToken,
            additionalInfo: notes,
            ticketSecondaryId: correction.Id);
        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, correction, cancellationToken);

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}
