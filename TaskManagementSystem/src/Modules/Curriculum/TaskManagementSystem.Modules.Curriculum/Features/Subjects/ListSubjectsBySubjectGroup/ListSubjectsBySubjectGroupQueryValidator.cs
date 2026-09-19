using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Subjects.ListSubjectsBySubjectGroup;

public sealed class ListSubjectsBySubjectGroupQueryValidator : AbstractValidator<ListSubjectsBySubjectGroupQuery>
{
    public ListSubjectsBySubjectGroupQueryValidator() => RuleFor(x => x.SubjectGroupId).GreaterThan(0);
}

