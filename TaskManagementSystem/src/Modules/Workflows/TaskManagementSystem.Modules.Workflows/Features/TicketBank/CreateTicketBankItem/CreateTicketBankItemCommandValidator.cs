using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.TicketBank.CreateTicketBankItem;

public sealed class CreateTicketBankItemCommandValidator : AbstractValidator<CreateTicketBankItemCommand>
{
    public CreateTicketBankItemCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TeamId).GreaterThan(0);
    }
}

