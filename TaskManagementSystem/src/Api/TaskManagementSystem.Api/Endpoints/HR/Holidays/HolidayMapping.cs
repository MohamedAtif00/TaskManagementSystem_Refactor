using TaskManagementSystem.Api.Contracts.HR;
using TaskManagementSystem.Modules.HR.Features.Holidays;

namespace TaskManagementSystem.Api.Endpoints.HR.Holidays;

internal static class HolidayMapping
{
    internal static HolidayResponse MapHoliday(HolidayResult holiday) =>
        new()
        {
            Id = holiday.Id,
            Name = holiday.Name,
            Description = holiday.Description,
            StartDate = holiday.StartDate,
            EndDate = holiday.EndDate,
            CreatedAt = holiday.CreatedAt,
            CreatedByUserId = holiday.CreatedByUserId
        };
}
