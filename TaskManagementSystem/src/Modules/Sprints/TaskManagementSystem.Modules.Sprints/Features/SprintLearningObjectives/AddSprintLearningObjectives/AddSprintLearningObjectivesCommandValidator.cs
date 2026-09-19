using FluentValidation;

namespace TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.AddSprintLearningObjectives;

public sealed class AddSprintLearningObjectivesCommandValidator : AbstractValidator<AddSprintLearningObjectivesCommand>
{
    public AddSprintLearningObjectivesCommandValidator()
    {
        RuleFor(x => x.SprintId).GreaterThan(0);
        RuleForEach(x => x.LearningObjectiveIds).GreaterThan(0);
    }
}

