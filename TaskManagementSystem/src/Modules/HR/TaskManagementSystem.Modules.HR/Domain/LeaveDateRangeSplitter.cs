namespace TaskManagementSystem.Modules.HR.Domain;

public static class LeaveDateRangeSplitter
{
    public static (DateTime? Start1, DateTime? End1, DateTime? Start2, DateTime? End2) SplitByWorkingDays(
        IReadOnlyList<DateTime> workingDays,
        int firstSegmentWorkingDays)
    {
        if (workingDays.Count == 0)
        {
            return (null, null, null, null);
        }

        if (firstSegmentWorkingDays <= 0)
        {
            return (null, null, workingDays[0], workingDays[^1]);
        }

        if (firstSegmentWorkingDays >= workingDays.Count)
        {
            return (workingDays[0], workingDays[^1], null, null);
        }

        return (
            workingDays[0],
            workingDays[firstSegmentWorkingDays - 1],
            workingDays[firstSegmentWorkingDays],
            workingDays[^1]);
    }
}
