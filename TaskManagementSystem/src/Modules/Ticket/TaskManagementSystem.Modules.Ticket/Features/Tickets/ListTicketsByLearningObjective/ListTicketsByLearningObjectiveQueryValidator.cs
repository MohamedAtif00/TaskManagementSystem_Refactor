using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsByLearningObjective;

public sealed class ListTicketsByLearningObjectiveQueryValidator : AbstractValidator<ListTicketsByLearningObjectiveQuery>
{
    public ListTicketsByLearningObjectiveQueryValidator() =>
        RuleFor(x => x.LearningObjectiveId).GreaterThan(0);
}

