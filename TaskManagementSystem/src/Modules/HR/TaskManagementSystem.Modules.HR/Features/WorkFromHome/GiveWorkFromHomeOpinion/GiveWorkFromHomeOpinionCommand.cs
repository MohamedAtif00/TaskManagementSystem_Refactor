using MediatR;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.GiveWorkFromHomeOpinion;

public sealed record GiveWorkFromHomeOpinionCommand(
    int ActorUserId,
    string ActorRole,
    int WorkFromHomeRequestId,
    bool IsApproved,
    string? Comment) : ICommand<Result<WorkFromHomeRequestResult>>;

public sealed class GiveWorkFromHomeOpinionCommandHandler(
    WorkFromHomeOpinionProcessor opinionProcessor,
    TimeProvider timeProvider)
    : IRequestHandler<GiveWorkFromHomeOpinionCommand, Result<WorkFromHomeRequestResult>>
{
    public async Task<Result<WorkFromHomeRequestResult>> Handle(
        GiveWorkFromHomeOpinionCommand request,
        CancellationToken cancellationToken)
    {
        var result = await opinionProcessor.ProcessAsync(
            request.ActorUserId,
            request.ActorRole,
            request.WorkFromHomeRequestId,
            request.IsApproved,
            request.Comment,
            timeProvider.GetUtcNow().UtcDateTime,
            cancellationToken);

        return result.IsSuccess
            ? Result.Ok(WorkFromHomeRequestResult.From(result.Value))
            : Result.Fail<WorkFromHomeRequestResult>(HrResultMapper.ToApplicationError(result.Error));
    }
}
