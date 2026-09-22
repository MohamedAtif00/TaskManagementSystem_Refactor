using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Ticket.Application;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.GetJumpPoints;

public sealed class GetJumpPointsQueryHandler(
    ITicketUnitOfWork unitOfWork,
    ILearningObjectiveLookup learningObjectiveLookup,
    IWorkflowStepLookup workflowStepLookup)
    : IRequestHandler<GetJumpPointsQuery, Result<IReadOnlyList<JumpPointResult>>>
{
    public async Task<Result<IReadOnlyList<JumpPointResult>>> Handle(
        GetJumpPointsQuery request,
        CancellationToken cancellationToken)
    {
        var ticket = await unitOfWork.Tickets.GetByIdAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Fail<IReadOnlyList<JumpPointResult>>(TicketErrors.TicketNotFound);
        }

        if (ticket.StepId is not int currentStepId)
        {
            return Result.Ok<IReadOnlyList<JumpPointResult>>(Array.Empty<JumpPointResult>());
        }

        var learningObjective = await learningObjectiveLookup.GetActiveByIdAsync(
            ticket.LearningObjectiveId,
            cancellationToken);
        if (learningObjective is null)
        {
            return Result.Fail<IReadOnlyList<JumpPointResult>>(TicketErrors.LearningObjectiveNotFound);
        }

        var points = await workflowStepLookup.ListStepsAheadAsync(
            learningObjective.SchemaId,
            currentStepId,
            cancellationToken);

        return Result.Ok<IReadOnlyList<JumpPointResult>>(points.Select(JumpPointResult.From).ToList());
    }
}
