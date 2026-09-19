using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.CreateLearningObjective;

public sealed class CreateLearningObjectiveCommandHandler(
    ICurriculumUnitOfWork unitOfWork,
    IWorkflowSchemaLookup workflowSchemaLookup)
    : IRequestHandler<CreateLearningObjectiveCommand, Result<LearningObjectiveDetailResult>>
{
    public async Task<Result<LearningObjectiveDetailResult>> Handle(CreateLearningObjectiveCommand request, CancellationToken cancellationToken)
    {
        if (!await workflowSchemaLookup.ActiveSchemaExistsAsync(request.SchemaId, cancellationToken))
            return Result.Fail<LearningObjectiveDetailResult>(CurriculumErrors.SchemaNotFound);
        if (await unitOfWork.LearningObjectives.ArchivedLessonExistsAsync(request.LessonId, cancellationToken))
            return Result.Fail<LearningObjectiveDetailResult>(CurriculumErrors.ParentArchived);
        if (!await unitOfWork.LearningObjectives.ActiveLessonExistsAsync(request.LessonId, cancellationToken))
            return Result.Fail<LearningObjectiveDetailResult>(CurriculumErrors.LessonNotFound);
        var createResult = LearningObjective.Create(
            request.Name,
            request.Tag,
            request.Template,
            request.Environment,
            request.LessonId,
            request.SchemaId,
            DateTime.UtcNow);
        if (!createResult.IsSuccess) return Result.Fail<LearningObjectiveDetailResult>(createResult.Error);
        await unitOfWork.LearningObjectives.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(LearningObjectiveDetailResult.From(createResult.Value));
    }
}

