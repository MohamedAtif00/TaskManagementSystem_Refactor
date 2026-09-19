using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Units.ListUnitsBySubject;

public sealed class ListUnitsBySubjectQueryValidator : AbstractValidator<ListUnitsBySubjectQuery>
{
    public ListUnitsBySubjectQueryValidator() => RuleFor(x => x.SubjectId).GreaterThan(0);
}

