using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Curriculum.Application;
using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Infrastructure.Persistence;

internal sealed class LearningObjectiveRepository(CurriculumDbContext context)
    : GenericRepository<LearningObjective, CurriculumDbContext>(context), ILearningObjectiveRepository
{
    public Task<LearningObjective?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(objective => objective.Id == id && !objective.Archived, cancellationToken);

    public Task<LearningObjective?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(objective => objective.Id == id && !objective.Archived, cancellationToken);

    public async Task<IReadOnlyList<LearningObjective>> ListActiveByParentIdAsync(
        int lessonId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(objective => objective.LessonId == lessonId && !objective.Archived)
            .OrderBy(objective => objective.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ActiveLessonExistsAsync(int lessonId, CancellationToken cancellationToken = default) =>
        Context.Lessons.AsNoTracking().AnyAsync(lesson => lesson.Id == lessonId && !lesson.Archived, cancellationToken);

    public Task<bool> ArchivedLessonExistsAsync(int lessonId, CancellationToken cancellationToken = default) =>
        Context.Lessons.AsNoTracking().AnyAsync(lesson => lesson.Id == lessonId && lesson.Archived, cancellationToken);

    public Task AddAsync(LearningObjective learningObjective, CancellationToken cancellationToken = default) =>
        AddEntityAsync(learningObjective, cancellationToken);
}
