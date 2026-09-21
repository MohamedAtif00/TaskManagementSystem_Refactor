using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySubject;

public sealed class ListTicketsBySubjectQueryValidator : AbstractValidator<ListTicketsBySubjectQuery>
{
    public ListTicketsBySubjectQueryValidator()
    {
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleFor(x => x.LearningObjectiveId).GreaterThan(0).When(x => x.LearningObjectiveId.HasValue);
        RuleForEach(x => x.Statuses).IsInEnum().When(x => x.Statuses is { Count: > 0 });
        When(x => x.Page.HasValue, () =>
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).NotNull().InclusiveBetween(1, 100);
        });
    }
}
