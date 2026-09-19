using FluentValidation;

namespace TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.RemoveSprintLearningObjective;

public sealed class RemoveSprintLearningObjectiveCommandValidator : AbstractValidator<RemoveSprintLearningObjectiveCommand>
{
    public RemoveSprintLearningObjectiveCommandValidator()
    {
        RuleFor(x => x.SprintId).GreaterThan(0);
        RuleFor(x => x.LearningObjectiveId).GreaterThan(0);
    }
}

