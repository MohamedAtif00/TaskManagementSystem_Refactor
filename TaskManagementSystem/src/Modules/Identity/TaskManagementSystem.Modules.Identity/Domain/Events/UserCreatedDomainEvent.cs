using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.Identity.Domain.Events;

public sealed record UserCreatedDomainEvent(
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
    int OldAnnualBalance) : DomainEventBase;
