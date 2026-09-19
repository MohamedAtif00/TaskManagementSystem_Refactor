using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.UpdateUnit;

public sealed class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

