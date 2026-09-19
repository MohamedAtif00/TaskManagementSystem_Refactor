using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.CreateSubject;

public sealed class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectCommandValidator()
    {
        RuleFor(x => x.SubjectGroupId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

