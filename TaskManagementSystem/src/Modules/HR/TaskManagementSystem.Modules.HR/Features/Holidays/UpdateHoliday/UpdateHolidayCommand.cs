using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Holidays;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Holidays.UpdateHoliday;

public sealed record UpdateHolidayCommand(
    int HolidayId,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate) : ICommand<Result<HolidayResult>>;

