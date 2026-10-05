using TaskManagementSystem.BuildingBlocks.Application;
using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Features.Leave.UpdateEmployeeBalance;

public sealed record UpdateEmployeeBalanceCommand(
    int UserId,
    string ViewerRole,
    int AnnualLeave,
    int AnnualLeaveMax,
    int EmergencyLeave,
    int EmergencyLeaveMax,
    int SickLeave,
    int Permission,
    int PermissionMax,
    int WorkFromHome,
    int WorkFromHomeMax,
    int FromNextBalanceDaysUsed) : ICommand<Result<NoValue>>;
