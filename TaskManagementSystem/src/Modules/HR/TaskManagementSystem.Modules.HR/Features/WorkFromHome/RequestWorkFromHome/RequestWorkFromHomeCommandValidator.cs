using FluentValidation;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.RequestWorkFromHome;

public sealed class RequestWorkFromHomeCommandValidator : AbstractValidator<RequestWorkFromHomeCommand>
{
    public RequestWorkFromHomeCommandValidator()
    {
        RuleFor(command => command.NoteForManager).MaximumLength(1000);
    }
}
