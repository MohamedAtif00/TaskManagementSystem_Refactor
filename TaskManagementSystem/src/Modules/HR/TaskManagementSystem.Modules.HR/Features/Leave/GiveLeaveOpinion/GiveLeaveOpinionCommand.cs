using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.GiveLeaveOpinion;

public sealed record GiveLeaveOpinionCommand(
    int ActorUserId,
    string ActorRole,
    int LeaveRequestId,
    bool IsApproved,
    string? Comment) : ICommand<Result<LeaveRequestResult>>;

public sealed class GiveLeaveOpinionCommandHandler(
    LeaveOpinionProcessor opinionProcessor,
    TimeProvider timeProvider)
    : IRequestHandler<GiveLeaveOpinionCommand, Result<LeaveRequestResult>>
{
    public async Task<Result<LeaveRequestResult>> Handle(
        GiveLeaveOpinionCommand request,
        CancellationToken cancellationToken)
    {
        var result = await opinionProcessor.ProcessAsync(
            request.ActorUserId,
            request.ActorRole,
            request.LeaveRequestId,
            request.IsApproved,
            request.Comment,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        return result.IsSuccess
            ? Result.Ok(LeaveRequestResult.From(result.Value))
            : Result.Fail<LeaveRequestResult>(HrResultMapper.ToApplicationError(result.Error));
    }
}
