using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.Curriculum.Application;

namespace TaskManagementSystem.Modules.Curriculum.Features.LearningObjectives.ListLearningObjectivesByLesson;

public sealed record ListLearningObjectivesByLessonQuery(int LessonId) : IQuery<Result<IReadOnlyList<LearningObjectiveListItemResult>>>;

