IF NOT EXISTS (SELECT 1 FROM [organization].[Teams] WHERE [Name] = N'Integration Test Team')
BEGIN
    INSERT INTO [organization].[Teams] ([Name], [Archived])
    VALUES (N'Integration Test Team', 0);
END
GO

DECLARE @TeamId INT = (SELECT TOP 1 [Id] FROM [organization].[Teams] WHERE [Name] = N'Integration Test Team');

IF NOT EXISTS (SELECT 1 FROM [identity].[Users] WHERE [Code] = N'TST001')
BEGIN
    INSERT INTO [identity].[Users]
    (
        [Name],
        [Code],
        [HR_code],
        [Role],
        [AccountType],
        [OnBoard],
        [Archived],
        [TeamId],
        [Annual_leave],
        [Annual_leave_MAX],
        [Emergency_leave],
        [Emergency_leave_MAX],
        [Sick_leave],
        [Permission],
        [Permission_MAX],
        [WorkFromHome],
        [WorkFromHome_MAX],
        [FromNextBalanceDaysUsed],
        [OldAnnualBalance]
    )
    VALUES
    (
        N'Integration Test User',
        N'TST001',
        N'999999',
        4,
        0,
        0,
        0,
        @TeamId,
        0, 30, 0, 5, 0, 0, 10, 0, 5, 0, 0
    );
END
GO

DECLARE @SeededUserId INT = (SELECT TOP 1 [Id] FROM [identity].[Users] WHERE [Code] = N'TST001');

IF @SeededUserId IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [hr].[EmployeeBalances] WHERE [UserId] = @SeededUserId)
BEGIN
    INSERT INTO [hr].[EmployeeBalances]
    (
        [UserId],
        [TeamId],
        [TeamleaderId],
        [Role],
        [AnnualLeave],
        [AnnualLeaveMax],
        [EmergencyLeave],
        [EmergencyLeaveMax],
        [SickLeave],
        [Permission],
        [PermissionMax],
        [WorkFromHome],
        [WorkFromHomeMax],
        [FromNextBalanceDaysUsed],
        [OldAnnualBalance]
    )
    SELECT
        u.[Id],
        u.[TeamId],
        u.[TeamleaderId],
        u.[Role],
        u.[Annual_leave],
        u.[Annual_leave_MAX],
        u.[Emergency_leave],
        u.[Emergency_leave_MAX],
        u.[Sick_leave],
        u.[Permission],
        u.[Permission_MAX],
        u.[WorkFromHome],
        u.[WorkFromHome_MAX],
        u.[FromNextBalanceDaysUsed],
        u.[OldAnnualBalance]
    FROM [identity].[Users] AS u
    WHERE u.[Id] = @SeededUserId;
END
GO
