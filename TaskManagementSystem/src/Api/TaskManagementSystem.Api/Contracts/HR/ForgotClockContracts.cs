namespace TaskManagementSystem.Api.Contracts.HR;

public sealed record CreateForgotClockRequest(
    string PunchType,
    DateTime AttendanceDate,
    string IntendedTime,
    string? Reason);

public sealed class ForgotClockRequestResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string PunchType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime AttendanceDate { get; set; }

    public string IntendedTime { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<OpinionResponse>? Opinions { get; set; }
}

public sealed class ForgotClockRequestListResponse
{
    public List<ForgotClockRequestResponse> Items { get; set; } = [];

    public int TotalCount { get; set; }
}

public sealed record GiveForgotClockOpinionRequest(
    bool IsApproved,
    string? Comment);

public sealed record GiveBulkForgotClockOpinionRequest(
    IReadOnlyList<int> ForgotClockRequestIds,
    bool IsApproved,
    string? Comment);

public sealed class BulkForgotClockOpinionResponse
{
    public int Succeeded { get; set; }

    public int Failed { get; set; }

    public List<int> FailedForgotClockRequestIds { get; set; } = [];
}
