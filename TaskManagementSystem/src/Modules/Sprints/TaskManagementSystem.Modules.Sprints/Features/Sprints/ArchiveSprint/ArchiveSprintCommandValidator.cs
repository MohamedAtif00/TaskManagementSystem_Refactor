using FluentValidation;

namespace TaskManagementSystem.Modules.Sprints.Features.Sprints.ArchiveSprint;

public sealed class ArchiveSprintCommandValidator : AbstractValidator<ArchiveSprintCommand>
{
    public ArchiveSprintCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

