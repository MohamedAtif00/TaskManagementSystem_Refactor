using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.UpdateLesson;

public sealed class UpdateLessonCommandValidator : AbstractValidator<UpdateLessonCommand>
{
    public UpdateLessonCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

