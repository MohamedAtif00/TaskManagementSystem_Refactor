using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.CreateSubjectGroup;

public sealed class CreateSubjectGroupCommandValidator : AbstractValidator<CreateSubjectGroupCommand>
{
    public CreateSubjectGroupCommandValidator()
    {
        RuleFor(x => x.TermId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

