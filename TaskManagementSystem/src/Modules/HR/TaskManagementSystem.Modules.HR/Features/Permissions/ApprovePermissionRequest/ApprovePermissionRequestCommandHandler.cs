using MediatR;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Features.Permissions.GivePermissionOpinion;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.ApprovePermissionRequest;

public sealed class ApprovePermissionRequestCommandHandler(IMediator mediator)
    : IRequestHandler<ApprovePermissionRequestCommand, Result<PermissionRequestResult>>
{
    public async Task<Result<PermissionRequestResult>> Handle(
        ApprovePermissionRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ActorRole != "Owner")
        {
            return Result.Fail<PermissionRequestResult>(HrErrors.HrApproveNotAuthorized);
        }

        return await mediator.Send(
            new GivePermissionOpinionCommand(
                request.ActorUserId,
                request.ActorRole,
                request.PermissionId,
                true,
                null),
            cancellationToken);
    }
}

