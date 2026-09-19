using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Projects.ArchiveProject;

public sealed class ArchiveProjectCommandValidator : AbstractValidator<ArchiveProjectCommand>
{
    public ArchiveProjectCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

