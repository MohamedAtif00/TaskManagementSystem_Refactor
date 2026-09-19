using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.UpdateSubjectGroup;

public sealed class UpdateSubjectGroupCommandValidator : AbstractValidator<UpdateSubjectGroupCommand>
{
    public UpdateSubjectGroupCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

