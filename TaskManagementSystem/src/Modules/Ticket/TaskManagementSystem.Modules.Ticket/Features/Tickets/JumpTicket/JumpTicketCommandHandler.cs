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

        if (!await workflowStepLookup.IsStepAheadAsync(
                learningObjective.SchemaId,
                currentStepId,
                request.StepId,
                cancellationToken))
        {
            return Result.Fail<TicketDetailResult>(TicketErrors.StepNotFound);
        }

        var jumpResult = ticket.JumpToStep(request.StepId);
        if (!jumpResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(jumpResult.Error);
        }

        await activityWriter.WriteAsync(ticket.Id, TicketActivityType.Jump, request.ActorUserId, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}
