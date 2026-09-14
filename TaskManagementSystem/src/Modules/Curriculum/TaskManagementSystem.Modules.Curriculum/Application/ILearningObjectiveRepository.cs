using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface ILearningObjectiveRepository
{
    Task<LearningObjective?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<LearningObjective?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LearningObjective>> ListActiveByParentIdAsync(int lessonId, CancellationToken cancellationToken = default);
    Task<bool> ActiveLessonExistsAsync(int lessonId, CancellationToken cancellationToken = default);
    Task<bool> ArchivedLessonExistsAsync(int lessonId, CancellationToken cancellationToken = default);
    Task AddAsync(LearningObjective learningObjective, CancellationToken cancellationToken = default);
}
