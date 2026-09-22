using MediatR;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.GiveForgotClockOpinion;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.ApproveForgotClockRequest;

public sealed class ApproveForgotClockRequestCommandHandler(IMediator mediator)
    : IRequestHandler<ApproveForgotClockRequestCommand, Result<ForgotClockRequestResult>>
{
    public async Task<Result<ForgotClockRequestResult>> Handle(
        ApproveForgotClockRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ActorRole != "Owner")
        {
            return Result.Fail<ForgotClockRequestResult>(HrErrors.HrApproveNotAuthorized);
        }

        return await mediator.Send(
            new GiveForgotClockOpinionCommand(
                request.ActorUserId,
                request.ActorRole,
                request.ForgotClockRequestId,
                true,
                null),
            cancellationToken);
    }
}

