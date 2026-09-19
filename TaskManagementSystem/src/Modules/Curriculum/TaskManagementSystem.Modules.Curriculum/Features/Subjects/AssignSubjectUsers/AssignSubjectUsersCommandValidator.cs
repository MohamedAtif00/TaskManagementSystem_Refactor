using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.AssignSubjectUsers;

public sealed class AssignSubjectUsersCommandValidator : AbstractValidator<AssignSubjectUsersCommand>
{
    public AssignSubjectUsersCommandValidator()
    {
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleForEach(x => x.UserIds).GreaterThan(0);
    }
}

