using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.ArchiveAcademicYear;

public sealed class ArchiveAcademicYearCommandValidator : AbstractValidator<ArchiveAcademicYearCommand>
{
    public ArchiveAcademicYearCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

