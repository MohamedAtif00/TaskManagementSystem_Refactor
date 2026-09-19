using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.ArchiveSubjectGroup;

public sealed class ArchiveSubjectGroupCommandValidator : AbstractValidator<ArchiveSubjectGroupCommand>
{
    public ArchiveSubjectGroupCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

