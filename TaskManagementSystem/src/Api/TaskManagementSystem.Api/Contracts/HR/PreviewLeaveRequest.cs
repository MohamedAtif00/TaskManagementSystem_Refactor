namespace TaskManagementSystem.Api.Contracts.HR;

public sealed record PreviewLeaveRequest(
    DateTime StartDate,
    DateTime EndDate);
