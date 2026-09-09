-- Manual re-seed for integration / local dev test user.
-- Same data as Migrations/011_identity_SeedUsers.sql (keep in sync).

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
