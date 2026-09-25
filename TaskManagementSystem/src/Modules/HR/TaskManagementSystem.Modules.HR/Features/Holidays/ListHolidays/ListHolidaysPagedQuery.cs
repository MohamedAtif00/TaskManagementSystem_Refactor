using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Application.Paging;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Holidays.ListHolidays;

public sealed record ListHolidaysPagedQuery(
    DateTime? FromDate,
    DateTime? ToDate,
    int? Page,
    int? PageSize) : IQuery<Result<PageListResult<HolidayResult>>>;
