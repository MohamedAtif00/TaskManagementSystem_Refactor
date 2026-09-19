using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.SubjectGroups.ListSubjectGroupsByTerm;

public sealed class ListSubjectGroupsByTermQueryValidator : AbstractValidator<ListSubjectGroupsByTermQuery>
{
    public ListSubjectGroupsByTermQueryValidator() => RuleFor(x => x.TermId).GreaterThan(0);
}

