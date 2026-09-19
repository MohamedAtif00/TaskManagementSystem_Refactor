using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Terms.ArchiveTerm;

public sealed class ArchiveTermCommandValidator : AbstractValidator<ArchiveTermCommand>
{
    public ArchiveTermCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

