using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Features.Holidays;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Holidays.CreateHoliday;

public sealed record CreateHolidayCommand(
    int CreatedByUserId,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate) : ICommand<Result<HolidayResult>>;

