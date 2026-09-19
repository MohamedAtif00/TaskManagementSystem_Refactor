using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GiveBulkLeaveOpinion;

public sealed class GiveBulkLeaveOpinionCommandHandler(
    LeaveOpinionProcessor opinionProcessor,
    TimeProvider timeProvider)
    : IRequestHandler<GiveBulkLeaveOpinionCommand, Result<BulkLeaveOpinionResult>>
{
    public async Task<Result<BulkLeaveOpinionResult>> Handle(
        GiveBulkLeaveOpinionCommand request,
        CancellationToken cancellationToken)
    {
        var failed = new List<int>();
        var succeeded = 0;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var leaveRequestId in request.LeaveRequestIds)
        {
            var result = await opinionProcessor.ProcessAsync(
                request.ActorUserId,
                request.ActorRole,
                leaveRequestId,
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
                failed.Add(leaveRequestId);
            }
        }

        return Result.Ok(new BulkLeaveOpinionResult(succeeded, failed.Count, failed));
    }
}

