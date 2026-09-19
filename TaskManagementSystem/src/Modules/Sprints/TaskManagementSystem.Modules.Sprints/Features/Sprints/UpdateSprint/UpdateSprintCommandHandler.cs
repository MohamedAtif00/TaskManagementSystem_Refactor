using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Sprints.Application;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.UpdateSprint;

public sealed class UpdateSprintCommandHandler(ISprintsUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSprintCommand, Result<SprintDetailResult>>
{
    public async Task<Result<SprintDetailResult>> Handle(UpdateSprintCommand request, CancellationToken cancellationToken)
    {
        var sprint = await unitOfWork.Sprints.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (sprint is null)
        {
            return Result.Fail<SprintDetailResult>(SprintsErrors.SprintNotFound);
        }

        var updateResult = sprint.Update(request.Name, request.Description, request.StartDate, request.EndDate);
        if (!updateResult.IsSuccess)
        {
            return Result.Fail<SprintDetailResult>(updateResult.Error);
        }

        var learningObjectiveIds = await unitOfWork.SprintLearningObjectives
            .ListLearningObjectiveIdsBySprintIdAsync(request.Id, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(SprintDetailResult.From(sprint, learningObjectiveIds));
    }
}

