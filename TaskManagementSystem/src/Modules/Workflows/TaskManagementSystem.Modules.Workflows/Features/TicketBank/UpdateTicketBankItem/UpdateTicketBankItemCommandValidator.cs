using FluentValidation;

namespace TaskManagementSystem.Modules.Workflows.Features.TicketBank.UpdateTicketBankItem;

public sealed class UpdateTicketBankItemCommandValidator : AbstractValidator<UpdateTicketBankItemCommand>
{
    public UpdateTicketBankItemCommandValidator()
    {
        RuleFor(x => x.TicketBankId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TeamId).GreaterThan(0);
    }
}

