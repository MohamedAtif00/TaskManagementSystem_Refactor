using MediatR;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Queries;

namespace TaskManagementSystem.Modules.HR.Features.Holidays.ListHolidays;

public sealed class ListHolidaysPagedQueryHandler(HolidayListQueries queries)
    : IRequestHandler<ListHolidaysPagedQuery, Result<PageListResult<HolidayResult>>>
{
    public Task<Result<PageListResult<HolidayResult>>> Handle(
        ListHolidaysPagedQuery request,
        CancellationToken cancellationToken) =>
        queries.ListPagedAsync(
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize,
            cancellationToken);
}
