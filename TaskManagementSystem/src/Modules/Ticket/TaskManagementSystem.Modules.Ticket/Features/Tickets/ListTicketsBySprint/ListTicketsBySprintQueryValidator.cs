using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.ListTicketsBySprint;

public sealed class ListTicketsBySprintQueryValidator : AbstractValidator<ListTicketsBySprintQuery>
{
    public ListTicketsBySprintQueryValidator() => RuleFor(x => x.SprintId).GreaterThan(0);
}

