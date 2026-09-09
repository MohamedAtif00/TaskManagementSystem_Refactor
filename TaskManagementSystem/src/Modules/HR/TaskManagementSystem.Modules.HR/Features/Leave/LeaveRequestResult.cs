using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave;

public sealed record LeaveRequestResult(
    int Id,
    int UserId,
    LeaveType Type,
    LeaveStatus Status,
    DateTime StartDate,
    DateTime EndDate,
    int WorkingDays,
    string? Reason,
    string? NoteForManager,
    DateTime? DateCreated,
    string? MedicalCertificateFileName = null,
    IReadOnlyList<OpinionResult>? Opinions = null)
{
    public static LeaveRequestResult From(LeaveRequest leaveRequest, IReadOnlyList<OpinionResult>? opinions = null) =>
        new(
            leaveRequest.Id,
            leaveRequest.UserId,
            leaveRequest.Type,
            leaveRequest.Status,
            leaveRequest.StartDate,
            leaveRequest.EndDate,
            leaveRequest.WorkingDays,
            leaveRequest.Reason,
            leaveRequest.NoteForManager,
            leaveRequest.DateCreated,
            leaveRequest.MedicalCertificateFileName,
            opinions);
}

public sealed record OpinionResult(
    int Id,
    int UserId,
    bool IsApproved,
    string? Comment,
    DateTime CreatedAt)
{
    public static OpinionResult From(Opinion opinion) =>
        new(opinion.Id, opinion.UserId, opinion.IsApproved, opinion.Comment, opinion.CreatedAt);
}

public sealed record LeaveRequestListResult(
    IReadOnlyList<LeaveRequestResult> Items,
    int TotalCount);
