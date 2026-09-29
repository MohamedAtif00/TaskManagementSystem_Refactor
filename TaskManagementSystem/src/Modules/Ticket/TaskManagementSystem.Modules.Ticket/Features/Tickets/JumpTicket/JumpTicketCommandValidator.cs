using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.JumpTicket;

public sealed class JumpTicketCommandValidator : AbstractValidator<JumpTicketCommand>
{
    public JumpTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.StepIds).NotEmpty();
        RuleForEach(x => x.StepIds).GreaterThan(0);
        RuleFor(x => x.ActorUserId).GreaterThan(0);
        RuleFor(x => x.ActorRole).NotEmpty();
    }
}
