using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.RemoveSprintLearningObjective;

public sealed class RemoveSprintLearningObjectiveCommandHandler(ISprintsUnitOfWork unitOfWork)
    : IRequestHandler<RemoveSprintLearningObjectiveCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        RemoveSprintLearningObjectiveCommand request,
        CancellationToken cancellationToken)
    {
        if (await unitOfWork.Sprints.GetByIdAsync(request.SprintId, cancellationToken) is null)
        {
            return Result.Fail<NoValue>(SprintsErrors.SprintNotFound);
        }

        var existingIds = await unitOfWork.SprintLearningObjectives
            .ListLearningObjectiveIdsBySprintIdAsync(request.SprintId, cancellationToken);

        if (!existingIds.Contains(request.LearningObjectiveId))
        {
            return Result.Fail<NoValue>(SprintsErrors.SprintLearningObjectiveNotFound);
        }

        await unitOfWork.SprintLearningObjectives.RemoveLearningObjectiveAsync(
            request.SprintId,
            request.LearningObjectiveId,
            cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}

