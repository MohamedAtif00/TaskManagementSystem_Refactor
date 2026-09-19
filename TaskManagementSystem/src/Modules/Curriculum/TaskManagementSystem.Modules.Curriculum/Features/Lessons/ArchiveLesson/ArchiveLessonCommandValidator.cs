using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.ArchiveLesson;

public sealed class ArchiveLessonCommandValidator : AbstractValidator<ArchiveLessonCommand>
{
    public ArchiveLessonCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

