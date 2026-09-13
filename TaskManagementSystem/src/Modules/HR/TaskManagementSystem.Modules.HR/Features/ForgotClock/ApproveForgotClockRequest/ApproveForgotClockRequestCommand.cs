using MediatR;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.ForgotClock.GiveForgotClockOpinion;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.ApproveForgotClockRequest;

public sealed record ApproveForgotClockRequestCommand(int ActorUserId, int ForgotClockRequestId)
    : ICommand<Result<ForgotClockRequestResult>>;

public sealed class ApproveForgotClockRequestCommandHandler(IMediator mediator)
    : IRequestHandler<ApproveForgotClockRequestCommand, Result<ForgotClockRequestResult>>
{
    public Task<Result<ForgotClockRequestResult>> Handle(
        ApproveForgotClockRequestCommand request,
        CancellationToken cancellationToken) =>
        mediator.Send(
            new GiveForgotClockOpinionCommand(
                request.ActorUserId,
                "Owner",
                request.ForgotClockRequestId,
                true,
                null),
            cancellationToken);
}
