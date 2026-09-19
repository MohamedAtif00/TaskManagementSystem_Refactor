using FluentValidation;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.UpdateSprint;

public sealed class UpdateSprintCommandValidator : AbstractValidator<UpdateSprintCommand>
{
    public UpdateSprintCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

