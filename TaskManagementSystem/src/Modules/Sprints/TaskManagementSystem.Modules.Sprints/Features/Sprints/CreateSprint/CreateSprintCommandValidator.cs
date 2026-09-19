using FluentValidation;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.CreateSprint;

public sealed class CreateSprintCommandValidator : AbstractValidator<CreateSprintCommand>
{
    public CreateSprintCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

