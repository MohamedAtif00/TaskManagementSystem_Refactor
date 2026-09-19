using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.AcademicYears.UpdateAcademicYear;

public sealed class UpdateAcademicYearCommandValidator : AbstractValidator<UpdateAcademicYearCommand>
{
    public UpdateAcademicYearCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

