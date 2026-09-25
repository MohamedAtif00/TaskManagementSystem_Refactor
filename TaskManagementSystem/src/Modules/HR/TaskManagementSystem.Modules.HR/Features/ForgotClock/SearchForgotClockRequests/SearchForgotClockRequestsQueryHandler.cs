using MediatR;
using TaskManagementSystem.Modules.HR.Features.ForgotClock;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.ForgotClock.SearchForgotClockRequests;

public sealed class SearchForgotClockRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<SearchForgotClockRequestsQuery, Result<ForgotClockRequestListResult>>
{
    public async Task<Result<ForgotClockRequestListResult>> Handle(
        SearchForgotClockRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var searchResult = await unitOfWork.ForgotClockRequests.SearchAsync(
            new ForgotClockRequestSearchCriteria(
                request.ViewerUserId,
                request.ViewerRole,
                request.ViewerTeamId,
                request.Page,
                request.PageSize,
                request.Search,
                request.Date,
                request.FromDate,
                request.ToDate,
                request.Status,
                request.PunchType,
                request.MyStatus),
            cancellationToken);

        var items = searchResult.Items.Select(r => ForgotClockRequestResult.From(r)).ToList();
        return Result.Ok(new ForgotClockRequestListResult(
            items,
            request.Page,
            request.PageSize,
            searchResult.TotalCount));
    }
}

