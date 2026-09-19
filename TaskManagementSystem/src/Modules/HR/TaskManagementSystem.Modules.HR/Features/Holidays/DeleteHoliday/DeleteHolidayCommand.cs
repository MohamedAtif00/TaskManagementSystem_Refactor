using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;
using TaskManagementSystem.Modules.HR.Application;

namespace TaskManagementSystem.Modules.HR.Features.Holidays.DeleteHoliday;

public sealed record DeleteHolidayCommand(int HolidayId) : ICommand<Result<NoValue>>;

