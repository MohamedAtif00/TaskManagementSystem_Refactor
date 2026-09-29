using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.RollbackTicket;

public sealed class RollbackTicketCommandValidator : AbstractValidator<RollbackTicketCommand>
{
    public RollbackTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.StepId).GreaterThan(0);
        RuleFor(x => x.Clarification).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.IssueNotes).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ActorUserId).GreaterThan(0);
        RuleFor(x => x.ActorRole).NotEmpty();
    }
}
