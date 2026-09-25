namespace TaskManagementSystem.Api.Contracts.HR;

public sealed record CreatePermissionRequest(
    string Type,
    DateTime PermissionDate,
    string FromTime,
    string ToTime,
    string? Reason);

public sealed class PermissionRequestResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime PermissionDate { get; set; }

    public string FromTime { get; set; } = string.Empty;

    public string ToTime { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<OpinionResponse>? Opinions { get; set; }
}

public sealed class PermissionRequestListResponse
{
    public List<PermissionRequestResponse> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}

public sealed record GivePermissionOpinionRequest(
    bool IsApproved,
    string? Comment);

public sealed record GiveBulkPermissionOpinionRequest(
    IReadOnlyList<int> PermissionIds,
    bool IsApproved,
    string? Comment);

public sealed class BulkPermissionOpinionResponse
{
    public int Succeeded { get; set; }

    public int Failed { get; set; }

    public List<int> FailedPermissionIds { get; set; } = [];
}
