using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.TaskBank.CreateTaskBankItem;

public sealed class CreateTaskBankItemCommandValidator : AbstractValidator<CreateTaskBankItemCommand>
{
    public CreateTaskBankItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TeamId).GreaterThan(0);
    }
}

