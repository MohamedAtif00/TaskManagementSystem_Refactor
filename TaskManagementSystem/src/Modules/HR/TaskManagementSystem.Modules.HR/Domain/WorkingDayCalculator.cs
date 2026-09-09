namespace TaskManagementSystem.Modules.HR.Domain;

public static class WorkingDayCalculator
{
    public static int Count(DateTime startDate, DateTime endDate)
    {
        var start = startDate.Date;
        var end = endDate.Date;

        if (end < start)
        {
            return 0;
        }

        var count = 0;
        for (var day = start; day <= end; day = day.AddDays(1))
        {
            if (IsWorkingDay(day))
            {
                count++;
            }
        }

        return count;
    }

    public static bool IsWorkingDay(DateTime date) =>
        date.DayOfWeek is not DayOfWeek.Friday and not DayOfWeek.Saturday;
}
