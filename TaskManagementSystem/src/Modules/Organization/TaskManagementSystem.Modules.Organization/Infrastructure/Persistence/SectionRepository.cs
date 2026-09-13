using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Organization.Application;
using TaskManagementSystem.Modules.Organization.Domain;

namespace TaskManagementSystem.Modules.Organization.Infrastructure.Persistence;

internal sealed class SectionRepository(OrganizationDbContext context)
    : GenericRepository<Section, OrganizationDbContext>(context), ISectionRepository
{
    public Task<Section?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(
            Set.Include(section => section.SectionTeams),
            section => section.Id == id && !section.Archived,
            cancellationToken);

    public Task<Section?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        Set.Include(section => section.SectionTeams)
            .FirstOrDefaultAsync(section => section.Id == id && !section.Archived, cancellationToken);

    public async Task<IReadOnlyList<Section>> ListActiveAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(section => !section.Archived)
            .OrderBy(section => section.Name)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsActiveByNameAsync(
        string name,
        int? excludeSectionId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToLowerInvariant();
        return ExistsReadOnlyAsync(
            section => !section.Archived
                && section.Name.ToLower() == normalized
                && (!excludeSectionId.HasValue || section.Id != excludeSectionId.Value),
            cancellationToken);
    }

    public Task AddAsync(Section section, CancellationToken cancellationToken = default) =>
        AddEntityAsync(section, cancellationToken);
}
