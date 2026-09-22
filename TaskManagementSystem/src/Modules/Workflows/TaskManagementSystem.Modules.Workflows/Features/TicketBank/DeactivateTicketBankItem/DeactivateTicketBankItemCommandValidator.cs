using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.TicketBank.DeactivateTicketBankItem;

public sealed class DeactivateTicketBankItemCommandValidator : AbstractValidator<DeactivateTicketBankItemCommand>
{
    public DeactivateTicketBankItemCommandValidator()
    {
        RuleFor(x => x.TicketBankId).GreaterThan(0);
    }
}

