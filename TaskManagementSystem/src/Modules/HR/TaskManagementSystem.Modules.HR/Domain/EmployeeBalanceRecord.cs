using TaskManagementSystem.BuildingBlocks.Domain;

namespace TaskManagementSystem.Modules.HR.Domain;

public sealed class EmployeeBalanceRecord : Entity, IAggregateRoot
{
    private EmployeeBalanceRecord()
    {
    }

    public int UserId { get; internal set; }

    public int? TeamId { get; internal set; }

    public int? TeamleaderId { get; internal set; }

    public int Role { get; internal set; }

    public int AnnualLeave { get; internal set; }

    public int AnnualLeaveMax { get; internal set; }

    public int EmergencyLeave { get; internal set; }

    public int EmergencyLeaveMax { get; internal set; }

    public int SickLeave { get; internal set; }

    public int Permission { get; internal set; }

    public int PermissionMax { get; internal set; }

    public int WorkFromHome { get; internal set; }

    public int WorkFromHomeMax { get; internal set; }

    public int FromNextBalanceDaysUsed { get; internal set; }

    public int OldAnnualBalance { get; internal set; }

    internal static EmployeeBalanceRecord CreateForPersistence() => new();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return UserId;
    }
}
