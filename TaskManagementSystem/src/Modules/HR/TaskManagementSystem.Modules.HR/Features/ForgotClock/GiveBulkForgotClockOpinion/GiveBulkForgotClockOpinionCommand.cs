using MediatR;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.GiveBulkForgotClockOpinion;

public sealed record GiveBulkForgotClockOpinionCommand(
    int ActorUserId,
    string ActorRole,
    IReadOnlyList<int> ForgotClockRequestIds,
    bool IsApproved,
    string? Comment) : IHrCommand<Result<BulkForgotClockOpinionResult>>;

public sealed class GiveBulkForgotClockOpinionCommandHandler(
    ForgotClockOpinionProcessor opinionProcessor,
    TimeProvider timeProvider)
    : IRequestHandler<GiveBulkForgotClockOpinionCommand, Result<BulkForgotClockOpinionResult>>
{
    public async Task<Result<BulkForgotClockOpinionResult>> Handle(
        GiveBulkForgotClockOpinionCommand request,
        CancellationToken cancellationToken)
    {
        var failed = new List<int>();
        var succeeded = 0;
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var forgotClockRequestId in request.ForgotClockRequestIds)
        {
            var result = await opinionProcessor.ProcessAsync(
                request.ActorUserId,
                request.ActorRole,
                forgotClockRequestId,
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
                failed.Add(forgotClockRequestId);
            }
        }

        return Result.Ok(new BulkForgotClockOpinionResult(succeeded, failed.Count, failed));
    }
}
