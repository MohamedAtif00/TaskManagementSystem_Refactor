using FluentValidation;

namespace TaskManagementSystem.Modules.Sprints.Features.SprintLearningObjectives.ListSprintLearningObjectives;

public sealed class ListSprintLearningObjectivesQueryValidator : AbstractValidator<ListSprintLearningObjectivesQuery>
{
    public ListSprintLearningObjectivesQueryValidator() => RuleFor(x => x.SprintId).GreaterThan(0);
}

