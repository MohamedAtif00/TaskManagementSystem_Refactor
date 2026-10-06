using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.GetSprintById;

public sealed class GetSprintByIdQueryHandler(
    ISprintsUnitOfWork unitOfWork,
    ILearningObjectiveLookup learningObjectiveLookup)
    : IRequestHandler<GetSprintByIdQuery, Result<SprintDetailResult>>
{
    public async Task<Result<SprintDetailResult>> Handle(GetSprintByIdQuery request, CancellationToken cancellationToken)
    {
        var sprint = await unitOfWork.Sprints.GetByIdAsync(request.Id, cancellationToken);
        if (sprint is null)
        {
            return Result.Fail<SprintDetailResult>(SprintsErrors.SprintNotFound);
        }

        var learningObjectiveIds = await unitOfWork.SprintLearningObjectives
            .ListLearningObjectiveIdsBySprintIdAsync(request.Id, cancellationToken);
        var visibleIds = await ActiveIdsAsync(learningObjectiveIds, cancellationToken);

        return Result.Ok(SprintDetailResult.From(sprint, visibleIds));
    }

    private async Task<IReadOnlyList<int>> ActiveIdsAsync(
        IReadOnlyList<int> learningObjectiveIds,
        CancellationToken cancellationToken)
    {
        var activeIds = (await learningObjectiveLookup.ListActiveIdsAsync(learningObjectiveIds, cancellationToken)).ToHashSet();
        return learningObjectiveIds.Where(activeIds.Contains).ToArray();
    }
}

