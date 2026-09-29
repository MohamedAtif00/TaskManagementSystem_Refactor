using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySprint;

public sealed class ListTicketsBySprintQueryValidator : AbstractValidator<ListTicketsBySprintQuery>
{
    public ListTicketsBySprintQueryValidator()
    {
        RuleFor(x => x.SprintId).GreaterThan(0);
        RuleFor(x => x.Filter).NotNull();
        RuleForEach(x => x.Filter.LearningObjectiveIds).GreaterThan(0).When(x => x.Filter.LearningObjectiveIds is { Count: > 0 });
        RuleForEach(x => x.Filter.UserIds).GreaterThan(0).When(x => x.Filter.UserIds is { Count: > 0 });
        RuleForEach(x => x.Filter.Statuses).IsInEnum().When(x => x.Filter.Statuses is { Count: > 0 });
        RuleForEach(x => x.Filter.Priorities).IsInEnum().When(x => x.Filter.Priorities is { Count: > 0 });
        When(x => x.Page.HasValue, () =>
        {
            RuleFor(x => x.Page).GreaterThan(0);
            RuleFor(x => x.PageSize).NotNull().InclusiveBetween(1, 100);
        });
    }
}
