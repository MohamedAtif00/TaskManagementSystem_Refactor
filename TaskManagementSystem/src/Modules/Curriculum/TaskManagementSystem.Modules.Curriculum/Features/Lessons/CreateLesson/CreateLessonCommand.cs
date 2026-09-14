using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.CreateLesson;

public sealed record CreateLessonCommand(int UnitId, string Name) : ICommand<Result<LessonDetailResult>>;

public sealed class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.UnitId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class CreateLessonCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<CreateLessonCommand, Result<LessonDetailResult>>
{
    public async Task<Result<LessonDetailResult>> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Lessons.ArchivedUnitExistsAsync(request.UnitId, cancellationToken))
            return Result.Fail<LessonDetailResult>(CurriculumErrors.ParentArchived);
        if (!await unitOfWork.Lessons.ActiveUnitExistsAsync(request.UnitId, cancellationToken))
            return Result.Fail<LessonDetailResult>(CurriculumErrors.UnitNotFound);
        var createResult = Lesson.Create(request.Name, request.UnitId);
        if (!createResult.IsSuccess) return Result.Fail<LessonDetailResult>(createResult.Error);
        await unitOfWork.Lessons.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok(LessonDetailResult.From(createResult.Value));
    }
}
