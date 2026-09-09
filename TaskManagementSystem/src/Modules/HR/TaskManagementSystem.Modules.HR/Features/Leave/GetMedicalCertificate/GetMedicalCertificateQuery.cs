using MediatR;
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

public sealed class GetMedicalCertificateQueryHandler(
    IHrUnitOfWork unitOfWork,
    IMedicalCertificateStorage medicalCertificateStorage)
    : IRequestHandler<GetMedicalCertificateQuery, Result<MedicalCertificateFileResult>>
{
    public async Task<Result<MedicalCertificateFileResult>> Handle(
        GetMedicalCertificateQuery request,
        CancellationToken cancellationToken)
    {
        var leaveRequest = await unitOfWork.LeaveRequests.GetByIdAsync(request.LeaveRequestId, cancellationToken);
        if (leaveRequest is null)
        {
            return Result.Fail<MedicalCertificateFileResult>(HrErrors.LeaveRequestNotFound);
        }

        if (leaveRequest.UserId != request.UserId && request.UserRole != "Owner")
        {
            return Result.Fail<MedicalCertificateFileResult>(HrErrors.LeaveRequestNotFound);
        }

        if (string.IsNullOrWhiteSpace(leaveRequest.MedicalCertificatePath))
        {
            return Result.Fail<MedicalCertificateFileResult>(HrErrors.LeaveMedicalNotFound);
        }

        var file = await medicalCertificateStorage.OpenAsync(
            leaveRequest.MedicalCertificatePath,
            leaveRequest.MedicalCertificateFileName,
            cancellationToken);

        if (file is null)
        {
            return Result.Fail<MedicalCertificateFileResult>(HrErrors.LeaveMedicalNotFound);
        }

        return Result.Ok(new MedicalCertificateFileResult(file.Value.Content, file.Value.FileName, file.Value.ContentType));
    }
}
