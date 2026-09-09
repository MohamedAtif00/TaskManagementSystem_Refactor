using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.BuildingBlocks.Persistence;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal sealed class HolidayRepository(HrDbContext context)
    : GenericRepository<PublicHoliday, HrDbContext>(context), IHolidayRepository
{
    public Task<PublicHoliday?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        FindReadOnlyAsync(holiday => holiday.Id == id, cancellationToken);

    public Task<PublicHoliday?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default) =>
        FindTrackedAsync(holiday => holiday.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PublicHoliday>> ListAsync(
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking();

        if (fromDate.HasValue)
        {
            query = query.Where(holiday => holiday.EndDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(holiday => holiday.StartDate <= toDate.Value.Date);
        }

        return await query
            .OrderBy(holiday => holiday.StartDate)
            .ThenBy(holiday => holiday.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PublicHoliday>> GetInRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var start = fromDate.Date;
        var end = toDate.Date;

        return await Set.AsNoTracking()
            .Where(holiday => holiday.StartDate <= end && holiday.EndDate >= start)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(PublicHoliday holiday, CancellationToken cancellationToken = default) =>
        AddEntityAsync(holiday, cancellationToken);

    public Task DeleteAsync(PublicHoliday holiday, CancellationToken cancellationToken = default)
    {
        Set.Remove(holiday);
        return Task.CompletedTask;
    }
}
