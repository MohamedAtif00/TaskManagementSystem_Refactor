using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.ArchiveUnit;

public sealed class ArchiveUnitCommandValidator : AbstractValidator<ArchiveUnitCommand>
{
    public ArchiveUnitCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

