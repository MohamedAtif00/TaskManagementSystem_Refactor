using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySubject;

public sealed class ListTicketsBySubjectQueryValidator : AbstractValidator<ListTicketsBySubjectQuery>
{
    public ListTicketsBySubjectQueryValidator() => RuleFor(x => x.SubjectId).GreaterThan(0);
}

