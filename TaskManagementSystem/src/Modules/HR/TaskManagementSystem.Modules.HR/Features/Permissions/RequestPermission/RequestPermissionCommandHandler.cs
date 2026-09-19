using MediatR;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.RequestPermission;

public sealed class RequestPermissionCommandHandler(
    IHrUnitOfWork unitOfWork,
    OrgLookupQueries orgLookupQueries,
    TimeProvider timeProvider)
    : IRequestHandler<RequestPermissionCommand, Result<PermissionRequestResult>>
{
    public async Task<Result<PermissionRequestResult>> Handle(
        RequestPermissionCommand request,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var balance = await unitOfWork.EmployeeBalances.GetByUserIdAsync(request.UserId, cancellationToken);
        if (balance is null)
        {
            return Result.Fail<PermissionRequestResult>(HrErrors.UserNotFound);
        }

        var pendingCount = await unitOfWork.PermissionRequests.CountPendingAsync(request.UserId, cancellationToken: cancellationToken);
        if (!balance.HasAvailablePermission(pendingCount))
        {
            return Result.Fail<PermissionRequestResult>(HrErrors.PermissionInsufficientBalance);
        }

        var (teamleaderId, sectionheadId) = await HrRequestContextHelper.ResolveApproversAsync(
            unitOfWork,
            orgLookupQueries,
            request.UserId,
            request.RequesterRole,
            cancellationToken);

        var createResult = PermissionRequest.Create(
            request.UserId,
            request.Type,
            request.PermissionDate,
            request.FromTime,
            request.ToTime,
            request.Reason,
            teamleaderId,
            sectionheadId,
            utcNow);

        if (!createResult.IsSuccess)
        {
            return Result.Fail<PermissionRequestResult>(HrResultMapper.ToApplicationError(createResult.Error));
        }

        await unitOfWork.PermissionRequests.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(PermissionRequestResult.From(createResult.Value));
    }
}

