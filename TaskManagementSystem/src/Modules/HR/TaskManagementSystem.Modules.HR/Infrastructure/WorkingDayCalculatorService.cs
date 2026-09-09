using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure;

public sealed class WorkingDayCalculatorService(IHolidayRepository holidayRepository) : IWorkingDayCalculator
{
    public async Task<int> CountAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var workingDays = await GetWorkingDaysInRangeAsync(startDate, endDate, cancellationToken);
        return workingDays.Count;
    }

    public async Task<bool> IsWorkingDayAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        if (!WorkingDayCalculator.IsWorkingDay(date))
        {
            return false;
        }

        var holidays = await holidayRepository.GetInRangeAsync(date.Date, date.Date, cancellationToken);
        return !IsHoliday(date, holidays);
    }

    public async Task<IReadOnlyList<DateTime>> GetWorkingDaysInRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var start = startDate.Date;
        var end = endDate.Date;
        if (end < start)
        {
            return [];
        }

        var holidays = await holidayRepository.GetInRangeAsync(start, end, cancellationToken);
        var workingDays = new List<DateTime>();

        for (var day = start; day <= end; day = day.AddDays(1))
        {
            if (WorkingDayCalculator.IsWorkingDay(day) && !IsHoliday(day, holidays))
            {
                workingDays.Add(day);
            }
        }

        return workingDays;
    }

    private static bool IsHoliday(DateTime date, IReadOnlyList<PublicHoliday> holidays) =>
        holidays.Any(holiday => date >= holiday.StartDate.Date && date <= holiday.EndDate.Date);
}
