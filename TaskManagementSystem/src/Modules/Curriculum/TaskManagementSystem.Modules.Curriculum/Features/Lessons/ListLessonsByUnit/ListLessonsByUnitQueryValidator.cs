using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.Lessons.ListLessonsByUnit;

public sealed class ListLessonsByUnitQueryValidator : AbstractValidator<ListLessonsByUnitQuery>
{
    public ListLessonsByUnitQueryValidator() => RuleFor(x => x.UnitId).GreaterThan(0);
}

