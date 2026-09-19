using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.CreateAcademicYear;

public sealed class CreateAcademicYearCommandValidator : AbstractValidator<CreateAcademicYearCommand>
{
    public CreateAcademicYearCommandValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
}

