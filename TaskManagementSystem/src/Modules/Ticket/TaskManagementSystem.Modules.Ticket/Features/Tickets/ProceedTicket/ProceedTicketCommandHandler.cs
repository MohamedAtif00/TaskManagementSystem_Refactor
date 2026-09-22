using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.BuildingBlocks.Infrastructure.Realtime;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ProceedTicket;

public sealed class ProceedTicketCommandHandler(
    ITicketUnitOfWork unitOfWork,
    ILearningObjectiveLookup learningObjectiveLookup,
    IWorkflowStepLookup workflowStepLookup,
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

        var nextStepId = await workflowStepLookup.GetNextStepIdAsync(
            learningObjective.SchemaId,
            currentStepId,
            cancellationToken);

        var proceedResult = ticket.ProceedToStep(nextStepId);
        if (!proceedResult.IsSuccess)
        {
            return Result.Fail<TicketDetailResult>(proceedResult.Error);
        }

        await unitOfWork.CommitAsync(cancellationToken);
        await TicketRealtimeNotifier.PublishUpdateAsync(realtimePublisher, ticket, cancellationToken);

        return Result.Ok(TicketDetailResult.From(ticket));
    }
}

