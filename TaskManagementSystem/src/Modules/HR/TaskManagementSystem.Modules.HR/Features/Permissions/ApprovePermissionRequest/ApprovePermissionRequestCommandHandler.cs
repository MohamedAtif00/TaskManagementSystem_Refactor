using MediatR;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Permissions.GivePermissionOpinion;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.ApprovePermissionRequest;

public sealed class ApprovePermissionRequestCommandHandler(IMediator mediator)
    : IRequestHandler<ApprovePermissionRequestCommand, Result<PermissionRequestResult>>
{
    public Task<Result<PermissionRequestResult>> Handle(
        ApprovePermissionRequestCommand request,
        CancellationToken cancellationToken) =>
        mediator.Send(
            new GivePermissionOpinionCommand(
                request.ActorUserId,
                "Owner",
                request.PermissionId,
                true,
                null),
            cancellationToken);
}

