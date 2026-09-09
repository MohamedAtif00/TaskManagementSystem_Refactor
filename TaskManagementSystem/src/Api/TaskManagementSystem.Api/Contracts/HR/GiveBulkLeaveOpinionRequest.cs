namespace TaskManagementSystem.Api.Contracts.HR;

public sealed record GiveBulkLeaveOpinionRequest(
    IReadOnlyList<int> LeaveRequestIds,
    bool IsApproved,
    string? Comment);
