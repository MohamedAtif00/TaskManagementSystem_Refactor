using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ProceedTicket;

public sealed class ProceedTicketCommandValidator : AbstractValidator<ProceedTicketCommand>
{
    public ProceedTicketCommandValidator() => RuleFor(x => x.TicketId).GreaterThan(0);
}

