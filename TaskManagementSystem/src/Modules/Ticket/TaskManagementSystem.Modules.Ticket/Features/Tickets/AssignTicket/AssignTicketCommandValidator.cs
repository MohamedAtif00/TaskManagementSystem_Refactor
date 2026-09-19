using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.AssignTicket;

public sealed class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
{
    public AssignTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

