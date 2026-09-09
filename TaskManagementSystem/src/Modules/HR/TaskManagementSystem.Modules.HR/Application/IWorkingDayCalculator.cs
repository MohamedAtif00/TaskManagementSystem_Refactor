namespace TaskManagementSystem.Modules.HR.Application;

public interface IWorkingDayCalculator
{
    Task<int> CountAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    Task<bool> IsWorkingDayAsync(DateTime date, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DateTime>> GetWorkingDaysInRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}
