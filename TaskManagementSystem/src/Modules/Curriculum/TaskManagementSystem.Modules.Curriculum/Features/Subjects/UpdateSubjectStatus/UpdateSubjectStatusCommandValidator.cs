using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.UpdateSubjectStatus;

public sealed class UpdateSubjectStatusCommandValidator : AbstractValidator<UpdateSubjectStatusCommand>
{
    public UpdateSubjectStatusCommandValidator() => RuleFor(x => x.SubjectId).GreaterThan(0);
}

