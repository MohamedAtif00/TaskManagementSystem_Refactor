using MediatR;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.GetRollbackPoints;

public sealed class GetRollbackPointsQueryHandler(
    ITicketUnitOfWork unitOfWork,
    ILearningObjectiveLookup learningObjectiveLookup,
    IWorkflowStepLookup workflowStepLookup)
    : IRequestHandler<GetRollbackPointsQuery, Result<IReadOnlyList<JumpPointResult>>>
{
    public async Task<Result<IReadOnlyList<JumpPointResult>>> Handle(
        GetRollbackPointsQuery request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<IReadOnlyList<JumpPointResult>>(TicketErrors.TicketNotFound);
        }

        if (ticket.StepId is not int currentStepId)
        {
            return Result.Ok<IReadOnlyList<JumpPointResult>>([]);
        }

        var learningObjective = await learningObjectiveLookup.GetActiveByIdAsync(
            ticket.LearningObjectiveId,
            cancellationToken);
        if (learningObjective is null)
        {
            return Result.Fail<IReadOnlyList<JumpPointResult>>(TicketErrors.LearningObjectiveNotFound);
        }

        var points = await workflowStepLookup.ListStepsBehindAsync(
            learningObjective.SchemaId,
            currentStepId,
            cancellationToken);

        return Result.Ok<IReadOnlyList<JumpPointResult>>(points.Select(JumpPointResult.From).ToList());
    }
}
