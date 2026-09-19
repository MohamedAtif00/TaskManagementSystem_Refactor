using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.TaskBank.DeactivateTaskBankItem;

public sealed class DeactivateTaskBankItemCommandValidator : AbstractValidator<DeactivateTaskBankItemCommand>
{
    public DeactivateTaskBankItemCommandValidator()
    {
        RuleFor(x => x.TaskBankId).GreaterThan(0);
    }
}

