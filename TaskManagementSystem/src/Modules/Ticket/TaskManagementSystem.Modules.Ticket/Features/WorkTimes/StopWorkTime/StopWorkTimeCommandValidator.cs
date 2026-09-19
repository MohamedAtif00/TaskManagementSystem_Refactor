using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.WorkTimes.StopWorkTime;

public sealed class StopWorkTimeCommandValidator : AbstractValidator<StopWorkTimeCommand>
{
    public StopWorkTimeCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

