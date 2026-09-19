using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure;

namespace TaskManagementSystem.Modules.HR.Features.Leave.RequestLeave;

public sealed record RequestLeaveCommand(
    int UserId,
    string RequesterRole,
    LeaveType Type,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason,
    string? NoteForManager,
    bool ConfirmFromNextBalance,
    bool HasMedicalCertificate,
    Stream? MedicalCertificateContent,
    string? MedicalCertificateFileName) : ICommand<Result<LeaveRequestResult>>;

