using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class WorkFromHomeRequestRepository(
    HrDbContext context,
    WorkFromHomeRequestSearchQueries searchQueries)
    : GenericRepository<WorkFromHomeRequest, HrDbContext>(context), IWorkFromHomeRequestRepository
{
    public Task AddAsync(WorkFromHomeRequest workFromHomeRequest, CancellationToken cancellationToken = default) =>
        AddEntityAsync(workFromHomeRequest, cancellationToken);

    public Task<WorkFromHomeRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(request => request.Id == id, cancellationToken);

    public Task<WorkFromHomeRequest?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(request => request.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkFromHomeRequest>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(request => request.UserId == userId)
            .OrderByDescending(request => request.DateCreated)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WorkFromHomeRequest>> GetPendingAsync(
        CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
            .Where(request => request.Status == WorkFromHomeStatus.Pending)
            .OrderBy(request => request.DateCreated)
            .ToListAsync(cancellationToken);

    public async Task<int> CountPendingAsync(
        int userId,
        int? excludeWorkFromHomeRequestId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
            .Where(request =>
                request.UserId == userId &&
                request.Status == WorkFromHomeStatus.Pending);

        if (excludeWorkFromHomeRequestId.HasValue)
        {
            query = query.Where(request => request.Id != excludeWorkFromHomeRequestId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public Task<bool> ExistsActiveForDateAsync(
        int userId,
        DateTime date,
        int? excludeWorkFromHomeRequestId = null,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
            .Where(request =>
                request.UserId == userId &&
                request.Date.Date == date.Date &&
                request.Status != WorkFromHomeStatus.Cancelled &&
                request.Status != WorkFromHomeStatus.Rejected);

        if (excludeWorkFromHomeRequestId.HasValue)
        {
            query = query.Where(request => request.Id != excludeWorkFromHomeRequestId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<WorkFromHomeRequestSearchResult> SearchAsync(
        WorkFromHomeRequestSearchCriteria criteria,
        CancellationToken cancellationToken = default) =>
        searchQueries.SearchAsync(criteria, cancellationToken);
}
