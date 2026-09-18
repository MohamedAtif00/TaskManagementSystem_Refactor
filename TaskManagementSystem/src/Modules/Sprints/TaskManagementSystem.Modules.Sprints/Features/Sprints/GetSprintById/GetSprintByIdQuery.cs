using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.GetSprintById;

public sealed record GetSprintByIdQuery(int Id) : IQuery<Result<SprintDetailResult>>;

public sealed class GetSprintByIdQueryValidator : AbstractValidator<GetSprintByIdQuery>
{
    public GetSprintByIdQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class GetSprintByIdQueryHandler(ISprintsUnitOfWork unitOfWork)
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

        return Result.Ok(SprintDetailResult.From(sprint, learningObjectiveIds));
    }
}
