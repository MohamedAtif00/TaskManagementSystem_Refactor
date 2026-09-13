using MediatR;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.GivePermissionOpinion;

public sealed record GivePermissionOpinionCommand(
    int ActorUserId,
    string ActorRole,
    int PermissionId,
    bool IsApproved,
    string? Comment) : ICommand<Result<PermissionRequestResult>>;

public sealed class GivePermissionOpinionCommandHandler(
    PermissionOpinionProcessor opinionProcessor,
    TimeProvider timeProvider)
    : IRequestHandler<GivePermissionOpinionCommand, Result<PermissionRequestResult>>
{
    public async Task<Result<PermissionRequestResult>> Handle(
        GivePermissionOpinionCommand request,
        CancellationToken cancellationToken)
    {
        var result = await opinionProcessor.ProcessAsync(
            request.ActorUserId,
            request.ActorRole,
            request.PermissionId,
            request.IsApproved,
            request.Comment,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        return result.IsSuccess
            ? Result.Ok(PermissionRequestResult.From(result.Value))
            : Result.Fail<PermissionRequestResult>(HrResultMapper.ToApplicationError(result.Error));
    }
}
