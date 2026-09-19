using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.Tickets.GetTicketById;

public sealed class GetTicketByIdQueryValidator : AbstractValidator<GetTicketByIdQuery>
{
    public GetTicketByIdQueryValidator() => RuleFor(x => x.TicketId).GreaterThan(0);
}

