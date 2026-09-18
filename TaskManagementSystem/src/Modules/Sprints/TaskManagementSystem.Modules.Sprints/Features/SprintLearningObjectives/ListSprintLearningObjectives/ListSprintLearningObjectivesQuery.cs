using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.ListSprintLearningObjectives;

public sealed record ListSprintLearningObjectivesQuery(int SprintId) : IQuery<Result<IReadOnlyList<int>>>;

public sealed class ListSprintLearningObjectivesQueryValidator : AbstractValidator<ListSprintLearningObjectivesQuery>
{
    public ListSprintLearningObjectivesQueryValidator() => RuleFor(x => x.SprintId).GreaterThan(0);
}

public sealed class ListSprintLearningObjectivesQueryHandler(ISprintsUnitOfWork unitOfWork)
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

        return Result.Ok<IReadOnlyList<int>>(learningObjectiveIds);
    }
}
