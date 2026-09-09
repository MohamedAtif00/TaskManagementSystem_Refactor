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
}
