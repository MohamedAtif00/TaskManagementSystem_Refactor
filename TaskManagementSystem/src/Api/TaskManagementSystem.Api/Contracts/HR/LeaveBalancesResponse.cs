namespace TaskManagementSystem.Api.Contracts.HR;

public sealed class LeaveBalancesResponse
{
    public int AnnualLeave { get; set; }

    public int AnnualLeaveMax { get; set; }

    public int AvailableAnnualLeave { get; set; }

    public int EmergencyLeave { get; set; }

    public int EmergencyLeaveMax { get; set; }

    public int AvailableEmergencyLeave { get; set; }

    public int SickLeave { get; set; }

    public int FromNextBalanceDaysUsed { get; set; }

    public int FromNextBalanceMaxDays { get; set; }

    public int Permission { get; set; }

    public int PermissionMax { get; set; }

    public int AvailablePermission { get; set; }

    public int WorkFromHome { get; set; }

    public int WorkFromHomeMax { get; set; }

    public int AvailableWorkFromHome { get; set; }
}

public sealed class MemberBalanceListItemResponse
{
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AnnualLeave { get; set; }
    public int AnnualLeaveMax { get; set; }
    public int EmergencyLeave { get; set; }
    public int EmergencyLeaveMax { get; set; }
    public int SickLeave { get; set; }
    public int Permission { get; set; }
    public int PermissionMax { get; set; }
    public int WorkFromHome { get; set; }
    public int WorkFromHomeMax { get; set; }
    public int FromNextBalanceDaysUsed { get; set; }
}

public sealed class MemberBalanceListPageResponse
{
    public IReadOnlyList<MemberBalanceListItemResponse> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
