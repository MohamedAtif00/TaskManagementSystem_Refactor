using MediatR;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GiveBulkWorkFromHomeOpinion;

public sealed class GiveBulkWorkFromHomeOpinionCommandHandler(
    WorkFromHomeOpinionProcessor opinionProcessor,
    TimeProvider timeProvider)
    : IRequestHandler<GiveBulkWorkFromHomeOpinionCommand, Result<BulkWorkFromHomeOpinionResult>>
{
    public async Task<Result<BulkWorkFromHomeOpinionResult>> Handle(
        GiveBulkWorkFromHomeOpinionCommand request,
        CancellationToken cancellationToken)
    {
        var failed = new List<int>();
        var succeeded = 0;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var workFromHomeRequestId in request.WorkFromHomeRequestIds)
        {
            var result = await opinionProcessor.ProcessAsync(
                request.ActorUserId,
                request.ActorRole,
                workFromHomeRequestId,
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
                failed.Add(workFromHomeRequestId);
            }
        }

        return Result.Ok(new BulkWorkFromHomeOpinionResult(succeeded, failed.Count, failed));
    }
}

