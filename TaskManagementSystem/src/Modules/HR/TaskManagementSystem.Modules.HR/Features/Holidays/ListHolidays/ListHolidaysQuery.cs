using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Holidays;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Holidays.ListHolidays;

public sealed record ListHolidaysQuery(DateTime? FromDate, DateTime? ToDate)
    : IQuery<Result<IReadOnlyList<HolidayResult>>>;

