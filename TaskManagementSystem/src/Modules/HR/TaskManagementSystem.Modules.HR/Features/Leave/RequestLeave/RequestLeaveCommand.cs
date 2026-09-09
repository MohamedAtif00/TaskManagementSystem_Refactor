using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
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

public sealed class RequestLeaveCommandHandler(
    IHrUnitOfWork unitOfWork,
    LeaveRequestPlanner planner,
    IMedicalCertificateStorage medicalCertificateStorage,
    TimeProvider timeProvider)
    : IRequestHandler<RequestLeaveCommand, Result<LeaveRequestResult>>
{
    public async Task<Result<LeaveRequestResult>> Handle(
        RequestLeaveCommand request,
        CancellationToken cancellationToken)
    {
        if (request.MedicalCertificateContent is not null &&
            (request.MedicalCertificateFileName is null ||
             !medicalCertificateStorage.IsValid(request.MedicalCertificateFileName, request.MedicalCertificateContent.Length)))
        {
            return Result.Fail<LeaveRequestResult>(HrErrors.LeaveMedicalInvalid);
        }

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var planResult = await planner.PlanAsync(
            unitOfWork,
            new LeaveRequestPlan(
                request.UserId,
                request.RequesterRole,
                request.Type,
                request.StartDate,
                request.EndDate,
                request.Reason,
                request.NoteForManager,
                request.ConfirmFromNextBalance,
                request.HasMedicalCertificate || request.MedicalCertificateContent is not null,
                utcNow),
            cancellationToken);

        if (!planResult.IsSuccess)
        {
            return Result.Fail<LeaveRequestResult>(HrResultMapper.ToApplicationError(planResult.Error));
        }

        var leaveRequests = planResult.Value.ToList();
        if (leaveRequests.Count == 1)
        {
            await unitOfWork.LeaveRequests.AddAsync(leaveRequests[0], cancellationToken);
        }
        else
        {
            await unitOfWork.LeaveRequests.AddRangeAsync(leaveRequests, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        var primary = leaveRequests[0];
        if (request.Type == LeaveType.Sick &&
            request.MedicalCertificateContent is not null &&
            request.MedicalCertificateFileName is not null)
        {
            var tracked = await unitOfWork.LeaveRequests.GetByIdTrackedAsync(primary.Id, cancellationToken);
            if (tracked is not null)
            {
                var path = await medicalCertificateStorage.SaveAsync(
                    request.MedicalCertificateContent,
                    request.MedicalCertificateFileName,
                    tracked.Id,
                    cancellationToken);
                tracked.SetMedicalCertificate(request.MedicalCertificateFileName, path);
                await unitOfWork.CommitAsync(cancellationToken);
                primary = tracked;
            }
        }

        return Result.Ok(LeaveRequestResult.From(primary));
    }
}
