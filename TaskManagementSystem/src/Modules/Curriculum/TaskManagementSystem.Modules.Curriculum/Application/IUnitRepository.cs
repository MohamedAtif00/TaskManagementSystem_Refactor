using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface IUnitRepository
{
    Task<Unit?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Unit?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Unit>> ListActiveByParentIdAsync(int subjectId, CancellationToken cancellationToken = default);
    Task<bool> ActiveSubjectExistsAsync(int subjectId, CancellationToken cancellationToken = default);
    Task<bool> ArchivedSubjectExistsAsync(int subjectId, CancellationToken cancellationToken = default);
    Task AddAsync(Unit unit, CancellationToken cancellationToken = default);
}
