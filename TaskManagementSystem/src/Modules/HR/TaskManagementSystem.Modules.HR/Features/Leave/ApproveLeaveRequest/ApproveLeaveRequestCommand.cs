using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Leave.GiveLeaveOpinion;

namespace TaskManagementSystem.Modules.HR.Features.Leave.ApproveLeaveRequest;

public sealed record ApproveLeaveRequestCommand(int ActorUserId, int LeaveRequestId)
    : ICommand<Result<LeaveRequestResult>>;

public sealed class ApproveLeaveRequestCommandHandler(IMediator mediator)
    : IRequestHandler<ApproveLeaveRequestCommand, Result<LeaveRequestResult>>
{
    public Task<Result<LeaveRequestResult>> Handle(
        ApproveLeaveRequestCommand request,
        CancellationToken cancellationToken) =>
        mediator.Send(
            new GiveLeaveOpinionCommand(
                request.ActorUserId,
                "Owner",
                request.LeaveRequestId,
                true,
                null),
            cancellationToken);
}
