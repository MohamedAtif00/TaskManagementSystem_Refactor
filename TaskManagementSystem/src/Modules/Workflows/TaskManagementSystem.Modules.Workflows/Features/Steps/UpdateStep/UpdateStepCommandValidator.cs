using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.Steps.UpdateStep;

public sealed class UpdateStepCommandValidator : AbstractValidator<UpdateStepCommand>
{
    public UpdateStepCommandValidator()
    {
        RuleFor(x => x.StepId).GreaterThan(0);
        RuleFor(x => x.TaskBankId).GreaterThan(0);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
    }
}

