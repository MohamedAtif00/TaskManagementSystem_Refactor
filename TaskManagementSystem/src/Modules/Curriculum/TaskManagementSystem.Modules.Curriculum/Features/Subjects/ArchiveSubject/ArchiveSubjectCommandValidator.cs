using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ArchiveSubject;

public sealed class ArchiveSubjectCommandValidator : AbstractValidator<ArchiveSubjectCommand>
{
    public ArchiveSubjectCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

