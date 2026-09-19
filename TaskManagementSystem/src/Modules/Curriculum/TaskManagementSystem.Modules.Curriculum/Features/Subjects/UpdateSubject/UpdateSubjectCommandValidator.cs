using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.UpdateSubject;

public sealed class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

