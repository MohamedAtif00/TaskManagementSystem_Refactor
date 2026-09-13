using FluentValidation;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.RequestForgotClock;

public sealed class RequestForgotClockCommandValidator : AbstractValidator<RequestForgotClockCommand>
{
    public RequestForgotClockCommandValidator()
    {
        RuleFor(command => command.Reason).MaximumLength(1000);
    }
}
