using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.ListSprintLearningObjectives;

public sealed class ListSprintLearningObjectivesQueryHandler(
    ISprintsUnitOfWork unitOfWork,
    ILearningObjectiveLookup learningObjectiveLookup)
    : IRequestHandler<ListSprintLearningObjectivesQuery, Result<IReadOnlyList<int>>>
{
    public async Task<Result<IReadOnlyList<int>>> Handle(
        ListSprintLearningObjectivesQuery request,
        CancellationToken cancellationToken)
    {
        if (await unitOfWork.Sprints.GetByIdAsync(request.SprintId, cancellationToken) is null)
        {
            return Result.Fail<IReadOnlyList<int>>(SprintsErrors.SprintNotFound);
        }

        var learningObjectiveIds = await unitOfWork.SprintLearningObjectives
            .ListLearningObjectiveIdsBySprintIdAsync(request.SprintId, cancellationToken);
        var activeIds = (await learningObjectiveLookup.ListActiveIdsAsync(learningObjectiveIds, cancellationToken)).ToHashSet();

        return Result.Ok<IReadOnlyList<int>>(learningObjectiveIds.Where(activeIds.Contains).ToArray());
    }
}

