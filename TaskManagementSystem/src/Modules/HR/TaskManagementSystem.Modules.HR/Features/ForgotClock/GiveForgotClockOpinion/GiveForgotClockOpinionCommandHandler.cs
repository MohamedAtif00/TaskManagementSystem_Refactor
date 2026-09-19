using MediatR;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.GiveForgotClockOpinion;

public sealed class GiveForgotClockOpinionCommandHandler(
    ForgotClockOpinionProcessor opinionProcessor,
    TimeProvider timeProvider)
    : IRequestHandler<GiveForgotClockOpinionCommand, Result<ForgotClockRequestResult>>
{
    public async Task<Result<ForgotClockRequestResult>> Handle(
        GiveForgotClockOpinionCommand request,
        CancellationToken cancellationToken)
    {
        var result = await opinionProcessor.ProcessAsync(
            request.ActorUserId,
            request.ActorRole,
            request.ForgotClockRequestId,
            request.IsApproved,
            request.Comment,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        return result.IsSuccess
            ? Result.Ok(ForgotClockRequestResult.From(result.Value))
            : Result.Fail<ForgotClockRequestResult>(HrResultMapper.ToApplicationError(result.Error));
    }
}

