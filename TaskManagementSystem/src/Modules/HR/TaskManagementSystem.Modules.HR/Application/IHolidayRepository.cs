using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public interface IHolidayRepository
{
    Task<PublicHoliday?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PublicHoliday?> GetByIdTrackedAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicHoliday>> ListAsync(
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicHoliday>> GetInRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    Task AddAsync(PublicHoliday holiday, CancellationToken cancellationToken = default);

    Task DeleteAsync(PublicHoliday holiday, CancellationToken cancellationToken = default);
}
