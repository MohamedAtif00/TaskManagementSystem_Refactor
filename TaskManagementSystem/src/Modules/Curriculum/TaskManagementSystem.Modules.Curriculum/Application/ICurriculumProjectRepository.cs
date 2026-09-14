using TaskManagementSystem.Modules.Curriculum.Domain;

namespace TaskManagementSystem.Modules.Curriculum.Application;

public interface ICurriculumProjectRepository
{
    Task<CurriculumProject?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CurriculumProject?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CurriculumProject>> ListActiveByParentIdAsync(int yearId, CancellationToken cancellationToken = default);
    Task<bool> ActiveYearExistsAsync(int yearId, CancellationToken cancellationToken = default);
    Task<bool> ArchivedYearExistsAsync(int yearId, CancellationToken cancellationToken = default);
    Task AddAsync(CurriculumProject project, CancellationToken cancellationToken = default);
}
