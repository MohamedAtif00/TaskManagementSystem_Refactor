using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.CreateUnit;

public sealed class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

