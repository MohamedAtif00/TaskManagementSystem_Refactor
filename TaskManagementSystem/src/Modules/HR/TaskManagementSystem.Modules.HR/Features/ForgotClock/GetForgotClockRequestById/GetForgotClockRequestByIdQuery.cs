using MediatR;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.GetForgotClockRequestById;

public sealed record GetForgotClockRequestByIdQuery(int UserId, string UserRole, int ForgotClockRequestId)
    : IQuery<Result<ForgotClockRequestResult>>;

public sealed class GetForgotClockRequestByIdQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetForgotClockRequestByIdQuery, Result<ForgotClockRequestResult>>
{
    public async Task<Result<ForgotClockRequestResult>> Handle(
        GetForgotClockRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var forgotClock = await unitOfWork.ForgotClockRequests.GetByIdAsync(request.ForgotClockRequestId, cancellationToken);
        if (forgotClock is null)
        {
            return Result.Fail<ForgotClockRequestResult>(HrErrors.ForgotClockRequestNotFound);
        }

        if (forgotClock.UserId != request.UserId && request.UserRole != "Owner")
        {
            return Result.Fail<ForgotClockRequestResult>(HrErrors.ForgotClockRequestNotFound);
        }

        var opinions = await unitOfWork.Opinions.GetByForgotClockRequestIdAsync(request.ForgotClockRequestId, cancellationToken);
        var opinionResults = opinions.Select(OpinionResult.From).ToList();

        return Result.Ok(ForgotClockRequestResult.From(forgotClock, opinionResults));
    }
}
