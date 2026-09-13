using MediatR;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.CancelPermissionRequest;

public sealed record CancelPermissionRequestCommand(int UserId, int PermissionId)
    : ICommand<Result<PermissionRequestResult>>;

public sealed class CancelPermissionRequestCommandHandler(
    IHrUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<CancelPermissionRequestCommand, Result<PermissionRequestResult>>
{
    public async Task<Result<PermissionRequestResult>> Handle(
        CancelPermissionRequestCommand request,
        CancellationToken cancellationToken)
    {
        var permission = await unitOfWork.PermissionRequests.GetByIdTrackedAsync(
            request.PermissionId,
            cancellationToken);

        if (permission is null || permission.UserId != request.UserId)
        {
            return Result.Fail<PermissionRequestResult>(HrErrors.PermissionRequestNotFound);
        }

        var wasApproved = permission.Status == PermissionStatus.Approved;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        var cancelResult = permission.Cancel(utcNow, wasApproved);
        if (!cancelResult.IsSuccess)
        {
            return Result.Fail<PermissionRequestResult>(HrResultMapper.ToApplicationError(cancelResult.Error));
        }

        if (wasApproved)
        {
            await unitOfWork.EmployeeBalances.RefundPermissionAsync(permission.UserId, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(PermissionRequestResult.From(permission));
    }
}
