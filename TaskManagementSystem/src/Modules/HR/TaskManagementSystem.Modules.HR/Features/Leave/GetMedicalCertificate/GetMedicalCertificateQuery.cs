using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GetMedicalCertificate;

public sealed record GetMedicalCertificateQuery(
    int UserId,
    string UserRole,
    int LeaveRequestId) : IQuery<Result<MedicalCertificateFileResult>>;

public sealed record MedicalCertificateFileResult(
    Stream Content,
    string FileName,
    string ContentType);
