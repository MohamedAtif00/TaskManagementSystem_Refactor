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

    public int Permission { get; set; }

    public int PermissionMax { get; internal set; }

    public int WorkFromHome { get; set; }

    public int WorkFromHomeMax { get; internal set; }

    public int FromNextBalanceDaysUsed { get; internal set; }

    public int OldAnnualBalance { get; internal set; }

    public byte[] RowVersion { get; internal set; } = [];

    internal static EmployeeBalanceRecord CreateForPersistence() => new();

    public static EmployeeBalanceRecord CreateForUser(
        int userId,
        int? teamId,
        int? teamleaderId,
        int roleId,
        int annualLeave,
        int annualLeaveMax,
        int emergencyLeave,
        int emergencyLeaveMax,
        int sickLeave,
        int permissionBalance,
        int permissionMax,
        int workFromHome,
        int workFromHomeMax,
        int fromNextBalanceDaysUsed,
        int oldAnnualBalance) =>
        new()
        {
            UserId = userId,
            TeamId = teamId,
            TeamleaderId = teamleaderId,
            Role = roleId,
            AnnualLeave = annualLeave,
            AnnualLeaveMax = annualLeaveMax,
            EmergencyLeave = emergencyLeave,
            EmergencyLeaveMax = emergencyLeaveMax,
            SickLeave = sickLeave,
            Permission = permissionBalance,
            PermissionMax = permissionMax,
            WorkFromHome = workFromHome,
            WorkFromHomeMax = workFromHomeMax,
            FromNextBalanceDaysUsed = fromNextBalanceDaysUsed,
            OldAnnualBalance = oldAnnualBalance
        };

    public static EmployeeBalanceRecord CreateWithDefaultEntitlements(
        int userId,
        int? teamId,
        int? teamleaderId,
        int roleId) =>
        CreateForUser(userId, teamId, teamleaderId, roleId, 0, 30, 0, 5, 0, 0, 10, 0, 5, 0, 0);

    public void SyncMetadata(int? teamId, int? teamleaderId, int roleId)
    {
        TeamId = teamId;
        TeamleaderId = teamleaderId;
        Role = roleId;
    }

    public Result<NoValue> SetAmounts(
        int annualLeave,
        int annualLeaveMax,
        int emergencyLeave,
        int emergencyLeaveMax,
        int sickLeave,
        int permission,
        int permissionMax,
        int workFromHome,
        int workFromHomeMax,
        int fromNextBalanceDaysUsed,
        int fromNextBalanceMaxDays)
    {
        var values = new[]
        {
            annualLeave,
            annualLeaveMax,
            emergencyLeave,
            emergencyLeaveMax,
            sickLeave,
            permission,
            permissionMax,
            workFromHome,
            workFromHomeMax,
            fromNextBalanceDaysUsed
        };
        if (values.Any(value => value < 0))
        {
            return Result.Fail<NoValue>(new ResultError("balance_invalid", "Balance values must be zero or greater."));
        }

        if (annualLeave > annualLeaveMax
            || emergencyLeave > emergencyLeaveMax
            || permission > permissionMax
            || workFromHome > workFromHomeMax
            || fromNextBalanceDaysUsed > fromNextBalanceMaxDays)
        {
            return Result.Fail<NoValue>(new ResultError("balance_exceeds_max", "Used balance cannot be above its maximum."));
        }

        AnnualLeave = annualLeave;
        AnnualLeaveMax = annualLeaveMax;
        EmergencyLeave = emergencyLeave;
        EmergencyLeaveMax = emergencyLeaveMax;
        SickLeave = sickLeave;
        Permission = permission;
        PermissionMax = permissionMax;
        WorkFromHome = workFromHome;
        WorkFromHomeMax = workFromHomeMax;
        FromNextBalanceDaysUsed = fromNextBalanceDaysUsed;
        return Result.Ok();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return UserId;
    }
}
