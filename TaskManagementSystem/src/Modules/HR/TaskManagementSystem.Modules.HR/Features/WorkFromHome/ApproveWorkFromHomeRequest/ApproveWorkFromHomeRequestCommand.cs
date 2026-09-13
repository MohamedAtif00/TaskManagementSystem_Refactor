using MediatR;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.WorkFromHome.GiveWorkFromHomeOpinion;

namespace TaskManagementSystem.Modules.HR.Features.WorkFromHome.ApproveWorkFromHomeRequest;

public sealed record ApproveWorkFromHomeRequestCommand(int ActorUserId, int WorkFromHomeRequestId)
    : ICommand<Result<WorkFromHomeRequestResult>>;

public sealed class ApproveWorkFromHomeRequestCommandHandler(IMediator mediator)
    : IRequestHandler<ApproveWorkFromHomeRequestCommand, Result<WorkFromHomeRequestResult>>
{
    public Task<Result<WorkFromHomeRequestResult>> Handle(
        ApproveWorkFromHomeRequestCommand request,
        CancellationToken cancellationToken) =>
        mediator.Send(
            new GiveWorkFromHomeOpinionCommand(
                request.ActorUserId,
                "Owner",
                request.WorkFromHomeRequestId,
                true,
                null),
            cancellationToken);
}
