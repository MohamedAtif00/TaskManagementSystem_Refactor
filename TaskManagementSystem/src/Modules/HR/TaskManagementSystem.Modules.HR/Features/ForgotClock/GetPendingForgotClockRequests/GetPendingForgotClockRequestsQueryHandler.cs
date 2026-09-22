using MediatR;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.GetPendingForgotClockRequests;

public sealed class GetPendingForgotClockRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<GetPendingForgotClockRequestsQuery, Result<IReadOnlyList<ForgotClockRequestResult>>>
{
    public async Task<Result<IReadOnlyList<ForgotClockRequestResult>>> Handle(
        GetPendingForgotClockRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var searchResult = await unitOfWork.ForgotClockRequests.SearchAsync(
            new ForgotClockRequestSearchCriteria(
                request.ViewerUserId,
                request.ViewerRole,
                request.ViewerTeamId,
                Page: 1,
                PageSize: 100,
                Search: null,
                Date: null,
                FromDate: null,
                ToDate: null,
                Status: ForgotClockStatus.Pending,
                PunchType: null,
                MyStatus: null),
            cancellationToken);

        var results = searchResult.Items.Select(r => ForgotClockRequestResult.From(r)).ToList();
        return Result.Ok<IReadOnlyList<ForgotClockRequestResult>>(results);
    }
}

