using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.UpdateLearningObjective;

public sealed class UpdateLearningObjectiveCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<UpdateLearningObjectiveCommand, Result<LearningObjectiveDetailResult>>
{
    public async Task<Result<LearningObjectiveDetailResult>> Handle(UpdateLearningObjectiveCommand request, CancellationToken cancellationToken)
    {
        var objective = await unitOfWork.LearningObjectives.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (objective is null) return Result.Fail<LearningObjectiveDetailResult>(CurriculumErrors.LearningObjectiveNotFound);
        var updateResult = objective.Update(request.Name, request.Tag, request.Template, request.Environment, request.StartedAt, request.DoneAt);
        if (!updateResult.IsSuccess) return Result.Fail<LearningObjectiveDetailResult>(updateResult.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(LearningObjectiveDetailResult.From(objective));
    }
}

