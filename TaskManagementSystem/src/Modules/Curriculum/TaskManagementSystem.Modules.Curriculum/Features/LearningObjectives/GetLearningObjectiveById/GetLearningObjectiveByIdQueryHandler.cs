using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.GetLearningObjectiveById;

public sealed class GetLearningObjectiveByIdQueryHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<GetLearningObjectiveByIdQuery, Result<LearningObjectiveDetailResult>>
{
    public async Task<Result<LearningObjectiveDetailResult>> Handle(GetLearningObjectiveByIdQuery request, CancellationToken cancellationToken)
    {
        var objective = await unitOfWork.LearningObjectives.GetByIdAsync(request.Id, cancellationToken);
        return objective is null ? Result.Fail<LearningObjectiveDetailResult>(CurriculumErrors.LearningObjectiveNotFound) : Result.Ok(LearningObjectiveDetailResult.From(objective));
    }
}

