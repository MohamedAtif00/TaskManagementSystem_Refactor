using FluentValidation;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.ListLearningObjectivesByLesson;

public sealed class ListLearningObjectivesByLessonQueryValidator : AbstractValidator<ListLearningObjectivesByLessonQuery>
{
    public ListLearningObjectivesByLessonQueryValidator() => RuleFor(x => x.LessonId).GreaterThan(0);
}

