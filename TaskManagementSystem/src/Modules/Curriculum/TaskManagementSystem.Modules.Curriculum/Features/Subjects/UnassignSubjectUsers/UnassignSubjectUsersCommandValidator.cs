using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.UnassignSubjectUsers;

public sealed class UnassignSubjectUsersCommandValidator : AbstractValidator<UnassignSubjectUsersCommand>
{
    public UnassignSubjectUsersCommandValidator()
    {
        RuleFor(x => x.SubjectId).GreaterThan(0);
        RuleForEach(x => x.UserIds).GreaterThan(0);
    }
}

