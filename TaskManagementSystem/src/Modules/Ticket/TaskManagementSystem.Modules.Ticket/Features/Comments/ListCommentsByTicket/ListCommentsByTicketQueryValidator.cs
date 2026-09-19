using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Comments.ListCommentsByTicket;

public sealed class ListCommentsByTicketQueryValidator : AbstractValidator<ListCommentsByTicketQuery>
{
    public ListCommentsByTicketQueryValidator() => RuleFor(x => x.TicketId).GreaterThan(0);
}

