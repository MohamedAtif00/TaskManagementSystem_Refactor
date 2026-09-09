using FluentValidation;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.RequestLeave;

public sealed class RequestLeaveCommandValidator : AbstractValidator<RequestLeaveCommand>
{
    public RequestLeaveCommandValidator()
    {
        RuleFor(command => command.Reason).MaximumLength(1000);
        RuleFor(command => command.NoteForManager).MaximumLength(1000);
        RuleFor(command => command.EndDate).GreaterThanOrEqualTo(command => command.StartDate);
        RuleFor(command => command.Type).IsInEnum();
    }
}
