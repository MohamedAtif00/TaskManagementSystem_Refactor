using MediatR;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.RequestForgotClock;

public sealed class RequestForgotClockCommandHandler(
    IHrUnitOfWork unitOfWork,
    OrgLookupQueries orgLookupQueries,
    TimeProvider timeProvider)
    : IRequestHandler<RequestForgotClockCommand, Result<ForgotClockRequestResult>>
{
    public async Task<Result<ForgotClockRequestResult>> Handle(
        RequestForgotClockCommand request,
        CancellationToken cancellationToken)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        if (await unitOfWork.ForgotClockRequests.ExistsActiveForDateAndPunchTypeAsync(
                request.UserId,
                request.AttendanceDate,
                request.PunchType,
                cancellationToken: cancellationToken))
        {
            return Result.Fail<ForgotClockRequestResult>(HrErrors.ForgotClockDuplicatePunch);
        }

        var (teamleaderId, sectionheadId) = await HrRequestContextHelper.ResolveApproversAsync(
            unitOfWork,
            orgLookupQueries,
            request.UserId,
            request.RequesterRole,
            cancellationToken);

        var createResult = ForgotClockRequest.Create(
            request.UserId,
            request.PunchType,
            request.AttendanceDate,
            request.IntendedTime,
            request.Reason,
            teamleaderId,
            sectionheadId,
            utcNow);

        if (!createResult.IsSuccess)
        {
            return Result.Fail<ForgotClockRequestResult>(HrResultMapper.ToApplicationError(createResult.Error));
        }

        await unitOfWork.ForgotClockRequests.AddAsync(createResult.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(ForgotClockRequestResult.From(createResult.Value));
    }
}

