using MediatR;
using TaskManagementSystem.Modules.HR.Features.Leave;
using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.SearchLeaveRequests;

public sealed record SearchLeaveRequestsQuery(
    int ViewerUserId,
    string ViewerRole,
    int? ViewerTeamId,
    int Page,
    int PageSize,
    string? Search,
    DateTime? FromDate,
    DateTime? ToDate,
    LeaveStatus? Status,
    LeaveType? Type,
    string? MyStatus) : IQuery<Result<LeaveRequestListResult>>;

public sealed class SearchLeaveRequestsQueryHandler(IHrUnitOfWork unitOfWork)
    : IRequestHandler<SearchLeaveRequestsQuery, Result<LeaveRequestListResult>>
{
    public async Task<Result<LeaveRequestListResult>> Handle(
        SearchLeaveRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var searchResult = await unitOfWork.LeaveRequests.SearchAsync(
            new LeaveRequestSearchCriteria(
                request.ViewerUserId,
                request.ViewerRole,
                request.ViewerTeamId,
                request.Page,
                request.PageSize,
                request.Search,
                request.FromDate,
                request.ToDate,
                request.Status,
                request.Type,
                request.MyStatus),
            cancellationToken);

        var items = searchResult.Items.Select(lr => LeaveRequestResult.From(lr)).ToList();
        return Result.Ok(new LeaveRequestListResult(items, searchResult.TotalCount));
    }
}
