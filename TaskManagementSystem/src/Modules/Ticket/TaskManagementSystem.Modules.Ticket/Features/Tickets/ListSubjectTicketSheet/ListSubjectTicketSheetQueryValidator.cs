using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListSubjectTicketSheet;

public sealed class ListSubjectTicketSheetQueryValidator : AbstractValidator<ListSubjectTicketSheetQuery>
{
    public ListSubjectTicketSheetQueryValidator() => RuleFor(x => x.SubjectId).GreaterThan(0);
}
