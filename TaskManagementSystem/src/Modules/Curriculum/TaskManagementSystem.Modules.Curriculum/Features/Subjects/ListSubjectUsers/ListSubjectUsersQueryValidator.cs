using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectUsers;

public sealed class ListSubjectUsersQueryValidator : AbstractValidator<ListSubjectUsersQuery>
{
    public ListSubjectUsersQueryValidator() => RuleFor(x => x.SubjectId).GreaterThan(0);
}

