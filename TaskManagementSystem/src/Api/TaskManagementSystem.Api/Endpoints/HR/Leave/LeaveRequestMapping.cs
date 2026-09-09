using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;

namespace TaskManagementSystem.Api.Endpoints.HR.Leave;

internal static class LeaveRequestMapping
{
    internal static LeaveType? ParseLeaveType(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : Enum.TryParse<LeaveType>(value, true, out var leaveType)
                ? leaveType
                : null;

    internal static LeaveRequestResponse MapLeaveRequest(LeaveRequestResult leaveRequest) =>
        new()
        {
            Id = leaveRequest.Id,
            UserId = leaveRequest.UserId,
            Type = leaveRequest.Type.ToString(),
            Status = leaveRequest.Status.ToString(),
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            WorkingDays = leaveRequest.WorkingDays,
            Reason = leaveRequest.Reason,
            NoteForManager = leaveRequest.NoteForManager,
            DateCreated = leaveRequest.DateCreated,
            MedicalCertificateFileName = leaveRequest.MedicalCertificateFileName,
            Opinions = leaveRequest.Opinions?.Select(opinion => new OpinionResponse
            {
                Id = opinion.Id,
                UserId = opinion.UserId,
                IsApproved = opinion.IsApproved,
                Comment = opinion.Comment,
                CreatedAt = opinion.CreatedAt
            }).ToList()
        };
}
