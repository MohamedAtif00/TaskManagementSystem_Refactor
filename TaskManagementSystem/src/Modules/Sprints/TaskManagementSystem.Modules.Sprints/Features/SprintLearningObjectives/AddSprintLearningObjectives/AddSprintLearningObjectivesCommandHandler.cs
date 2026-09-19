using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.AddSprintLearningObjectives;

public sealed class AddSprintLearningObjectivesCommandHandler(
    ISprintsUnitOfWork unitOfWork,
    ILearningObjectiveLookup learningObjectiveLookup)
    : IRequestHandler<AddSprintLearningObjectivesCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(
        AddSprintLearningObjectivesCommand request,
        CancellationToken cancellationToken)
    {
        if (await unitOfWork.Sprints.GetByIdAsync(request.SprintId, cancellationToken) is null)
        {
            return Result.Fail<NoValue>(SprintsErrors.SprintNotFound);
        }

        var distinctIds = request.LearningObjectiveIds.Distinct().ToList();
        if (distinctIds.Count > 0 &&
            !await learningObjectiveLookup.ActiveLearningObjectivesExistAsync(distinctIds, cancellationToken))
        {
            return Result.Fail<NoValue>(SprintsErrors.LearningObjectiveNotFound);
        }

        await unitOfWork.SprintLearningObjectives.AddLearningObjectivesAsync(
            request.SprintId,
            distinctIds,
            cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}

