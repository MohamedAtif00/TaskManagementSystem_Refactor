using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.CompleteTicket;

public sealed class CompleteTicketCommandValidator : AbstractValidator<CompleteTicketCommand>
{
    public CompleteTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.ActorUserId).GreaterThan(0);
        RuleFor(x => x.ActorRole).NotEmpty();
    }
}

