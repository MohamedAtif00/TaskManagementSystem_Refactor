namespace TaskManagementSystem.Api.Contracts.HR;

public sealed record CreateLeaveRequest(
    string Type,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason,
    string? NoteForManager,
    bool ConfirmFromNextBalance = false);
