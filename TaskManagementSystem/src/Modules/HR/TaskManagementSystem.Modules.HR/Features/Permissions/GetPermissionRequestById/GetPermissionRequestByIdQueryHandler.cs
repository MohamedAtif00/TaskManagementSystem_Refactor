using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.GetPermissionRequestById;

public sealed class GetPermissionRequestByIdQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetPermissionRequestByIdQuery, Result<PermissionRequestResult>>
{
    public async Task<Result<PermissionRequestResult>> Handle(
        GetPermissionRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var permission = await unitOfWork.PermissionRequests.GetByIdAsync(request.PermissionId, cancellationToken);
        if (permission is null)
        {
            return Result.Fail<PermissionRequestResult>(HrErrors.PermissionRequestNotFound);
        }

        if (permission.UserId != request.UserId && request.UserRole != "Owner")
        {
            return Result.Fail<PermissionRequestResult>(HrErrors.PermissionRequestNotFound);
        }

        var opinions = await unitOfWork.Opinions.GetByPermissionIdAsync(request.PermissionId, cancellationToken);
        var opinionResults = opinions.Select(OpinionResult.From).ToList();

        return Result.Ok(PermissionRequestResult.From(permission, opinionResults));
    }
}

