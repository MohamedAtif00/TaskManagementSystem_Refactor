using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Application;

public interface ISectionRepository
{
    Task<Section?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Section?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Section>> ListActiveAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsActiveByNameAsync(string name, int? excludeSectionId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Section section, CancellationToken cancellationToken = default);
}
