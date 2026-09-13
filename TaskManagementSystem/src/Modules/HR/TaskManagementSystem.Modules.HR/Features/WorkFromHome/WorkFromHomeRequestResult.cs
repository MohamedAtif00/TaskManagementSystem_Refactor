using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome;

public sealed record WorkFromHomeRequestResult(
    int Id,
    int UserId,
    DateTime Date,
    WorkFromHomeStatus Status,
    string? NoteForManager,
    DateTime? DateCreated,
    IReadOnlyList<OpinionResult>? Opinions = null)
{
    public static WorkFromHomeRequestResult From(
        WorkFromHomeRequest request,
        IReadOnlyList<OpinionResult>? opinions = null) =>
        new(
            request.Id,
            request.UserId,
            request.Date,
            request.Status,
            request.NoteForManager,
            request.DateCreated,
            opinions);
}

public sealed record WorkFromHomeRequestListResult(
    IReadOnlyList<WorkFromHomeRequestResult> Items,
    int TotalCount);

public sealed record BulkWorkFromHomeOpinionResult(
    int Succeeded,
    int Failed,
    IReadOnlyList<int> FailedWorkFromHomeRequestIds);
