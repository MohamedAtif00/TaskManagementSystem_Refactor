namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence;

internal static class EmployeeBalanceSqlMutations
{
    public const string DeductAnnualLeave = """
        UPDATE [hr].[EmployeeBalances]
        SET [AnnualLeave] = [AnnualLeave] + @Days
        WHERE [UserId] = @UserId
          AND [AnnualLeave] + @Days <= [AnnualLeaveMax]
        """;

    public const string RefundAnnualLeave = """
        UPDATE [hr].[EmployeeBalances]
        SET [AnnualLeave] = CASE
            WHEN [AnnualLeave] >= @Days THEN [AnnualLeave] - @Days
            ELSE 0
        END
        WHERE [UserId] = @UserId
        """;

    public const string DeductEmergencyLeave = """
        UPDATE [hr].[EmployeeBalances]
        SET [EmergencyLeave] = [EmergencyLeave] + @Days
        WHERE [UserId] = @UserId
          AND [EmergencyLeave] + @Days <= [EmergencyLeaveMax]
        """;

    public const string RefundEmergencyLeave = """
        UPDATE [hr].[EmployeeBalances]
        SET [EmergencyLeave] = CASE
            WHEN [EmergencyLeave] >= @Days THEN [EmergencyLeave] - @Days
            ELSE 0
        END
        WHERE [UserId] = @UserId
        """;

    public const string DeductSickLeave = """
        UPDATE [hr].[EmployeeBalances]
        SET [SickLeave] = [SickLeave] + @Days
        WHERE [UserId] = @UserId
        """;

    public const string RefundSickLeave = """
        UPDATE [hr].[EmployeeBalances]
        SET [SickLeave] = CASE
            WHEN [SickLeave] >= @Days THEN [SickLeave] - @Days
            ELSE 0
        END
        WHERE [UserId] = @UserId
        """;

    public const string DeductFromNextBalance = """
        UPDATE [hr].[EmployeeBalances]
        SET [FromNextBalanceDaysUsed] = [FromNextBalanceDaysUsed] + @Days
        WHERE [UserId] = @UserId
          AND [FromNextBalanceDaysUsed] + @Days <= @MaxDays
        """;

    public const string RefundFromNextBalance = """
        UPDATE [hr].[EmployeeBalances]
        SET [FromNextBalanceDaysUsed] = CASE
            WHEN [FromNextBalanceDaysUsed] >= @Days THEN [FromNextBalanceDaysUsed] - @Days
            ELSE 0
        END
        WHERE [UserId] = @UserId
        """;

    public const string DeductPermission = """
        UPDATE [hr].[EmployeeBalances]
        SET [Permission] = [Permission] + 1
        WHERE [UserId] = @UserId
          AND [Permission] + 1 <= [PermissionMax]
        """;

    public const string RefundPermission = """
        UPDATE [hr].[EmployeeBalances]
        SET [Permission] = CASE
            WHEN [Permission] > 0 THEN [Permission] - 1
            ELSE 0
        END
        WHERE [UserId] = @UserId
        """;

    public const string DeductWorkFromHome = """
        UPDATE [hr].[EmployeeBalances]
        SET [WorkFromHome] = [WorkFromHome] + 1
        WHERE [UserId] = @UserId
          AND [WorkFromHome] + 1 <= [WorkFromHomeMax]
        """;

    public const string RefundWorkFromHome = """
        UPDATE [hr].[EmployeeBalances]
        SET [WorkFromHome] = CASE
            WHEN [WorkFromHome] > 0 THEN [WorkFromHome] - 1
            ELSE 0
        END
        WHERE [UserId] = @UserId
        """;
}
