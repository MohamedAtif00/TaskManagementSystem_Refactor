using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.UpdateLearningObjective;

public sealed class UpdateLearningObjectiveCommandValidator : AbstractValidator<UpdateLearningObjectiveCommand>
{
    public UpdateLearningObjectiveCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

