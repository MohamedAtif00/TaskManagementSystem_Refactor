using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.CreateLearningObjective;

public sealed class CreateLearningObjectiveCommandValidator : AbstractValidator<CreateLearningObjectiveCommand>
{
    public CreateLearningObjectiveCommandValidator()
    {
        RuleFor(x => x.LessonId).GreaterThan(0);
        RuleFor(x => x.SchemaId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

