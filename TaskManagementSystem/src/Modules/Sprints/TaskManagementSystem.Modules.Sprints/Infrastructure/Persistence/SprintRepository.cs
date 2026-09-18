using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.Sprints.Application;
using TaskManagementSystem.Modules.Sprints.Domain;

namespace TaskManagementSystem.Modules.Sprints.Infrastructure.Persistence;

internal sealed class SprintRepository(SprintsDbContext context)
    : GenericRepository<Sprint, SprintsDbContext>(context), ISprintRepository
{
    public Task<Sprint?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(sprint => sprint.Id == id && !sprint.IsArchived, cancellationToken);

    public Task<Sprint?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(sprint => sprint.Id == id && !sprint.IsArchived, cancellationToken);

    public async Task<IReadOnlyList<Sprint>> ListAsync(
        bool? archived,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking().AsQueryable();

        query = archived switch
        {
            true => query.Where(sprint => sprint.IsArchived),
            false => query.Where(sprint => !sprint.IsArchived),
            null => query.Where(sprint => !sprint.IsArchived)
        };

        return await query
            .OrderByDescending(sprint => sprint.StartDate)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Sprint sprint, CancellationToken cancellationToken = default) =>
        AddEntityAsync(sprint, cancellationToken);
}
