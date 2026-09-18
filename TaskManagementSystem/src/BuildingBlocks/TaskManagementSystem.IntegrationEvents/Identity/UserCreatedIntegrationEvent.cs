using TaskManagementSystem.BuildingBlocks.Application;

namespace TaskManagementSystem.IntegrationEvents.Identity;

public sealed record UserCreatedIntegrationEvent(
    Guid Id,
    DateTime OccurredOn,
    int UserId,
    int? TeamId,
    int? TeamleaderId,
    int RoleId,
    int AnnualLeave,
    int AnnualLeaveMax,
    int EmergencyLeave,
    int EmergencyLeaveMax,
    int SickLeave,
    int PermissionBalance,
    int PermissionMax,
    int WorkFromHome,
    int WorkFromHomeMax,
    int FromNextBalanceDaysUsed,
    int OldAnnualBalance) : IIntegrationEvent;
