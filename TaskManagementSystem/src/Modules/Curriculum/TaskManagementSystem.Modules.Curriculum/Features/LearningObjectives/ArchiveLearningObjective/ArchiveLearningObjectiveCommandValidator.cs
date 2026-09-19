using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.ArchiveLearningObjective;

public sealed class ArchiveLearningObjectiveCommandValidator : AbstractValidator<ArchiveLearningObjectiveCommand>
{
    public ArchiveLearningObjectiveCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

