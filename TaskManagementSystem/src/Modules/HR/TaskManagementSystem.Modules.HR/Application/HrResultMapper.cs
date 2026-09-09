using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public static class HrResultMapper
{
    public static ResultError ToApplicationError(ResultError domainError) =>
        domainError.Code switch
        {
            "leave_invalid_dates" => HrErrors.InvalidDates,
            "leave_cannot_cancel" => HrErrors.LeaveCannotCancel,
            "leave_not_pending" => HrErrors.LeaveNotPending,
            "holiday_invalid_dates" => HrErrors.HolidayInvalidDates,
            "holiday_invalid_name" => HrErrors.HolidayInvalidDates,
            _ => domainError
        };
}
