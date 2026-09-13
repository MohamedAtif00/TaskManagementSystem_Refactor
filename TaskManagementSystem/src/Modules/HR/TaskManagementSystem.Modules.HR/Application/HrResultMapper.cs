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
            "permission_invalid_dates" => HrErrors.PermissionInvalidDates,
            "permission_invalid_times" => HrErrors.PermissionInvalidTimes,
            "permission_cannot_cancel" => HrErrors.PermissionCannotCancel,
            "permission_not_pending" => HrErrors.PermissionNotPending,
            "work_from_home_invalid_date" => HrErrors.WorkFromHomeInvalidDate,
            "work_from_home_cannot_cancel" => HrErrors.WorkFromHomeCannotCancel,
            "work_from_home_not_pending" => HrErrors.WorkFromHomeNotPending,
            _ => domainError
        };
}
