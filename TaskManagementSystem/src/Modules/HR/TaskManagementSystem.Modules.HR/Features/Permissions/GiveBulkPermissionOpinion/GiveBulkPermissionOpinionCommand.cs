using MediatR;
using TaskManagementSystem.Modules.HR.Features.Permissions;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Permissions.GiveBulkPermissionOpinion;

public sealed record GiveBulkPermissionOpinionCommand(
    int ActorUserId,
    string ActorRole,
    IReadOnlyList<int> PermissionIds,
    bool IsApproved,
    string? Comment) : IHrCommand<Result<BulkPermissionOpinionResult>>;

public sealed class GiveBulkPermissionOpinionCommandHandler(
    PermissionOpinionProcessor opinionProcessor,
    TimeProvider timeProvider)
    : IRequestHandler<GiveBulkPermissionOpinionCommand, Result<BulkPermissionOpinionResult>>
{
    public async Task<Result<BulkPermissionOpinionResult>> Handle(
        GiveBulkPermissionOpinionCommand request,
        CancellationToken cancellationToken)
    {
        var failed = new List<int>();
        var succeeded = 0;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var permissionId in request.PermissionIds)
        {
            var result = await opinionProcessor.ProcessAsync(
                request.ActorUserId,
                request.ActorRole,
                permissionId,
                request.IsApproved,
                request.Comment,
                utcNow,
                cancellationToken);

            if (result.IsSuccess)
            {
                succeeded++;
            }
            else
            {
                failed.Add(permissionId);
            }
        }

        return Result.Ok(new BulkPermissionOpinionResult(succeeded, failed.Count, failed));
    }
}
