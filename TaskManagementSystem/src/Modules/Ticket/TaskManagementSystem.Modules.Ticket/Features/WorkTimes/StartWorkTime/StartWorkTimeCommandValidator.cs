using FluentValidation;

namespace TaskManagementSystem.Modules.Ticket.Features.WorkTimes.StartWorkTime;

public sealed class StartWorkTimeCommandValidator : AbstractValidator<StartWorkTimeCommand>
{
    public StartWorkTimeCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}

