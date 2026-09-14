using FluentValidation;
using MediatR;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.ArchiveLesson;

public sealed record ArchiveLessonCommand(int Id) : ICommand<Result<NoValue>>;

public sealed class ArchiveLessonCommandValidator : AbstractValidator<ArchiveLessonCommand>
{
    public ArchiveLessonCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class ArchiveLessonCommandHandler(ICurriculumUnitOfWork unitOfWork)
    : IRequestHandler<ArchiveLessonCommand, Result<NoValue>>
{
    public async Task<Result<NoValue>> Handle(ArchiveLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await unitOfWork.Lessons.GetByIdTrackedAsync(request.Id, cancellationToken);
        if (lesson is null) return Result.Fail<NoValue>(CurriculumErrors.LessonNotFound);
        var archiveResult = lesson.Archive();
        if (!archiveResult.IsSuccess) return archiveResult;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Ok();
    }
}
