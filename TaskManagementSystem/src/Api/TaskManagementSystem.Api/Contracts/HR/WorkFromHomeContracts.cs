namespace TaskManagementSystem.Api.Contracts.HR;

public sealed record CreateWorkFromHomeRequest(
    DateTime Date,
    string? NoteForManager);

public sealed class WorkFromHomeRequestResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime Date { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? NoteForManager { get; set; }

    public DateTime? DateCreated { get; set; }

    public List<OpinionResponse>? Opinions { get; set; }
}

public sealed class WorkFromHomeRequestListResponse
{
    public List<WorkFromHomeRequestResponse> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}

public sealed record GiveWorkFromHomeOpinionRequest(
    bool IsApproved,
    string? Comment);

public sealed record GiveBulkWorkFromHomeOpinionRequest(
    IReadOnlyList<int> WorkFromHomeRequestIds,
    bool IsApproved,
    string? Comment);

public sealed class BulkWorkFromHomeOpinionResponse
{
    public int Succeeded { get; set; }

    public int Failed { get; set; }

    public List<int> FailedWorkFromHomeRequestIds { get; set; } = [];
}
