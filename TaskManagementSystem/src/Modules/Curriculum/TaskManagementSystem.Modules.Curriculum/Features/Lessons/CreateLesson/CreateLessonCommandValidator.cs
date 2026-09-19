using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.CreateLesson;

public sealed class CreateLessonCommandValidator : AbstractValidator<CreateLessonCommand>
{
    public CreateLessonCommandValidator()
    {
        RuleFor(x => x.UnitId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

