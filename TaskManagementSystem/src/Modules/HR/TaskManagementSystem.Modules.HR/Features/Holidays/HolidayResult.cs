using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Holidays;

public sealed record HolidayResult(
    int Id,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    DateTime CreatedAt,
    int CreatedByUserId)
{
    public static HolidayResult From(PublicHoliday holiday) =>
        new(
            holiday.Id,
            holiday.Name,
            holiday.Description,
            holiday.StartDate,
            holiday.EndDate,
            holiday.CreatedAt,
            holiday.CreatedByUserId);
}
