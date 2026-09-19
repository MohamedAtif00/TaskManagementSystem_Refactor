-- Integration / local dev test user and team.
-- Run separately from migrations via DatabaseMigrator --seed or IntegrationTestDataSeeder.

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
        [TeamId]
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
        @TeamId
    );
END
GO

DECLARE @UserId INT = (SELECT TOP 1 [Id] FROM [identity].[Users] WHERE [Code] = N'TST001');

IF NOT EXISTS (SELECT 1 FROM [hr].[EmployeeBalances] WHERE [UserId] = @UserId)
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
        t.[TeamleaderId],
        u.[Role],
        0,
        30,
        0,
        5,
        0,
        0,
        10,
        0,
        5,
        0,
        0
    FROM [identity].[Users] AS u
    LEFT JOIN [organization].[Teams] AS t ON t.[Id] = u.[TeamId]
    WHERE u.[Id] = @UserId;
END
GO
