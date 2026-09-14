using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Lesson?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lesson>> ListActiveByParentIdAsync(int unitId, CancellationToken cancellationToken = default);
    Task<bool> ActiveUnitExistsAsync(int unitId, CancellationToken cancellationToken = default);
    Task<bool> ArchivedUnitExistsAsync(int unitId, CancellationToken cancellationToken = default);
    Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default);
}
