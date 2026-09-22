using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Features.Leave.GiveLeaveOpinion;

namespace TaskManagementSystem.Modules.HR.Features.Leave.ApproveLeaveRequest;

public sealed class ApproveLeaveRequestCommandHandler(IMediator mediator)
    : IRequestHandler<ApproveLeaveRequestCommand, Result<LeaveRequestResult>>
{
    public async Task<Result<LeaveRequestResult>> Handle(
        ApproveLeaveRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ActorRole != "Owner")
        {
            return Result.Fail<LeaveRequestResult>(HrErrors.HrApproveNotAuthorized);
        }

        return await mediator.Send(
            new GiveLeaveOpinionCommand(
                request.ActorUserId,
                request.ActorRole,
                request.LeaveRequestId,
                true,
                null),
            cancellationToken);
    }
}

