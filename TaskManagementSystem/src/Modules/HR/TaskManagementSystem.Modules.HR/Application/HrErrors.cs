using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Application;

public static class HrErrors
{
    public static ResultError InsufficientBalance =>
        new("leave_insufficient_balance", "Insufficient leave balance.");

    public static ResultError InvalidDates =>
        new("leave_invalid_dates", "Invalid leave dates.");

    public static ResultError LeaveTypeNotSupported =>
        new("leave_type_not_supported", "Leave type is not supported.");

    public static ResultError LeaveRequestNotFound =>
        new("leave_request_not_found", "Leave request not found.");

    public static ResultError LeaveNotPending =>
        new("leave_not_pending", "Only pending leave requests can be approved.");

    public static ResultError LeaveCannotCancel =>
        new("leave_cannot_cancel", "Leave request cannot be cancelled.");

    public static ResultError UserNotFound =>
        new("user_not_found", "User not found.");

    public static ResultError LeaveOpinionAlreadyGiven =>
        new("leave_opinion_already_given", "You have already given your opinion on this request.");

    public static ResultError LeaveOpinionNotAuthorized =>
        new("leave_opinion_not_authorized", "You are not authorized to give opinions on this request.");

    public static ResultError LeaveFromNextUnavailable =>
        new("leave_from_next_unavailable", "From next balance is not available.");

    public static ResultError LeaveFromNextConfirmationRequired =>
        new("leave_from_next_confirmation_required", "Annual leave exceeds available balance; confirm from-next usage.");

    public static ResultError LeaveEmergencyBlackout =>
        new("leave_emergency_blackout", "Emergency leave requests are temporarily disabled until the annual leave reset.");

    public static ResultError LeaveMedicalRequired =>
        new("leave_medical_required", "Medical certificate is required for sick leave longer than 3 days.");

    public static ResultError LeaveMedicalNotFound =>
        new("leave_medical_not_found", "Medical certificate not found.");

    public static ResultError LeaveMedicalInvalid =>
        new("leave_medical_invalid", "Invalid medical certificate file.");

    public static ResultError HolidayNotFound =>
        new("holiday_not_found", "Public holiday not found.");

    public static ResultError HolidayInvalidDates =>
        new("holiday_invalid_dates", "Invalid holiday dates.");

    public static ResultError PermissionInsufficientBalance =>
        new("permission_insufficient_balance", "Insufficient permission balance.");

    public static ResultError PermissionRequestNotFound =>
        new("permission_request_not_found", "Permission request not found.");

    public static ResultError PermissionNotPending =>
        new("permission_not_pending", "Only pending permission requests can be approved.");

    public static ResultError PermissionCannotCancel =>
        new("permission_cannot_cancel", "Permission request cannot be cancelled.");

    public static ResultError PermissionInvalidDates =>
        new("permission_invalid_dates", "Invalid permission date.");

    public static ResultError PermissionInvalidTimes =>
        new("permission_invalid_times", "Invalid permission times.");

    public static ResultError PermissionOpinionAlreadyGiven =>
        new("permission_opinion_already_given", "You have already given your opinion on this request.");

    public static ResultError PermissionOpinionNotAuthorized =>
        new("permission_opinion_not_authorized", "You are not authorized to give opinions on this request.");

    public static ResultError WorkFromHomeInsufficientBalance =>
        new("work_from_home_insufficient_balance", "Insufficient work from home balance.");

    public static ResultError WorkFromHomeRequestNotFound =>
        new("work_from_home_request_not_found", "Work from home request not found.");

    public static ResultError WorkFromHomeNotPending =>
        new("work_from_home_not_pending", "Only pending work from home requests can be approved.");

    public static ResultError WorkFromHomeCannotCancel =>
        new("work_from_home_cannot_cancel", "Work from home request cannot be cancelled.");

    public static ResultError WorkFromHomeInvalidDate =>
        new("work_from_home_invalid_date", "Invalid work from home date.");

    public static ResultError WorkFromHomeDuplicateDate =>
        new("work_from_home_duplicate_date", "You already have a work from home request for this date.");

    public static ResultError WorkFromHomeOpinionAlreadyGiven =>
        new("work_from_home_opinion_already_given", "You have already given your opinion on this request.");

    public static ResultError WorkFromHomeOpinionNotAuthorized =>
        new("work_from_home_opinion_not_authorized", "You are not authorized to give opinions on this request.");

    public static ResultError ForgotClockRequestNotFound =>
        new("forgot_clock_request_not_found", "Forgot clock request not found.");

    public static ResultError ForgotClockNotPending =>
        new("forgot_clock_not_pending", "Only pending forgot clock requests can be approved.");

    public static ResultError ForgotClockCannotCancel =>
        new("forgot_clock_cannot_cancel", "Forgot clock request cannot be cancelled.");

    public static ResultError ForgotClockInvalidDate =>
        new("forgot_clock_invalid_date", "Invalid attendance date.");

    public static ResultError ForgotClockDuplicatePunch =>
        new("forgot_clock_duplicate_punch", "You already have an active forgot clock request for this date and punch type.");

    public static ResultError ForgotClockOpinionAlreadyGiven =>
        new("forgot_clock_opinion_already_given", "You have already given your opinion on this request.");

    public static ResultError ForgotClockOpinionNotAuthorized =>
        new("forgot_clock_opinion_not_authorized", "You are not authorized to give opinions on this request.");
}
