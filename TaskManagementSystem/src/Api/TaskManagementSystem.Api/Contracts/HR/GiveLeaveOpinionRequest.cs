namespace TaskManagementSystem.Api.Contracts.HR;

public sealed record GiveLeaveOpinionRequest(
    bool IsApproved,
    string? Comment);
