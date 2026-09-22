using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.CreateTicket;

public sealed class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
{
    public CreateTicketCommandValidator()
    {
        RuleFor(x => x.LearningObjectiveId).GreaterThan(0);
        RuleFor(x => x.TicketBankItemId).GreaterThan(0);
    }
}

