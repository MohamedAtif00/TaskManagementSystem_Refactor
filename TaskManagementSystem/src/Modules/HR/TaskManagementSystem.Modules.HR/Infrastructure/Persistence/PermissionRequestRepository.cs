using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class PermissionRequestRepository(
    HrDbContext context,
    PermissionRequestSearchQueries searchQueries)
    : GenericRepository<PermissionRequest, HrDbContext>(context), IPermissionRequestRepository
{
    public Task AddAsync(PermissionRequest permissionRequest, CancellationToken cancellationToken = default) =>
        AddEntityAsync(permissionRequest, cancellationToken);

    public Task<PermissionRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(permission => permission.Id == id, cancellationToken);

    public Task<PermissionRequest?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(permission => permission.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PermissionRequest>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(permission => permission.UserId == userId)
            .OrderByDescending(permission => permission.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PermissionRequest>> GetPendingAsync(
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(permission => permission.Status == PermissionStatus.Pending)
            .OrderBy(permission => permission.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<int> CountPendingAsync(
        int userId,
        int? excludePermissionId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
            .Where(permission =>
                permission.UserId == userId &&
                permission.Status == PermissionStatus.Pending);

        if (excludePermissionId.HasValue)
        {
            query = query.Where(permission => permission.Id != excludePermissionId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public Task<PermissionRequestSearchResult> SearchAsync(
        PermissionRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default) =>
        searchQueries.SearchAsync(criteria, cancellationToken);
}
