using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.ArchiveStep;

public sealed class ArchiveStepCommandValidator : AbstractValidator<ArchiveStepCommand>
{
    public ArchiveStepCommandValidator()
    {
        RuleFor(x => x.StepId).GreaterThan(0);
    }
}

