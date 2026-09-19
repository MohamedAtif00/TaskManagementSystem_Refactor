using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.TaskBank.UpdateTaskBankItem;

public sealed class UpdateTaskBankItemCommandValidator : AbstractValidator<UpdateTaskBankItemCommand>
{
    public UpdateTaskBankItemCommandValidator()
    {
        RuleFor(x => x.TaskBankId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TeamId).GreaterThan(0);
    }
}

