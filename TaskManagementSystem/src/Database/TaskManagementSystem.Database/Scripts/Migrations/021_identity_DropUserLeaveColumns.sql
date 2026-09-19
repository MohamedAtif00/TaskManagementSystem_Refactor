-- Leave balances live only in hr.EmployeeBalances. Drop legacy columns from identity.Users.
-- No-op on fresh databases created from updated 003; cleans up dev DBs built from the old schema.

IF COL_LENGTH(N'identity.Users', N'Annual_leave') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP CONSTRAINT IF EXISTS [DF_identity_Users_Annual_leave];
    ALTER TABLE [identity].[Users] DROP COLUMN [Annual_leave];
END
GO

IF COL_LENGTH(N'identity.Users', N'Annual_leave_MAX') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP CONSTRAINT IF EXISTS [DF_identity_Users_Annual_leave_MAX];
    ALTER TABLE [identity].[Users] DROP COLUMN [Annual_leave_MAX];
END
GO

IF COL_LENGTH(N'identity.Users', N'Emergency_leave') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP CONSTRAINT IF EXISTS [DF_identity_Users_Emergency_leave];
    ALTER TABLE [identity].[Users] DROP COLUMN [Emergency_leave];
END
GO

IF COL_LENGTH(N'identity.Users', N'Emergency_leave_MAX') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP CONSTRAINT IF EXISTS [DF_identity_Users_Emergency_leave_MAX];
    ALTER TABLE [identity].[Users] DROP COLUMN [Emergency_leave_MAX];
END
GO

IF COL_LENGTH(N'identity.Users', N'Sick_leave') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP CONSTRAINT IF EXISTS [DF_identity_Users_Sick_leave];
    ALTER TABLE [identity].[Users] DROP COLUMN [Sick_leave];
END
GO

IF COL_LENGTH(N'identity.Users', N'Permission') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP CONSTRAINT IF EXISTS [DF_identity_Users_Permission];
    ALTER TABLE [identity].[Users] DROP COLUMN [Permission];
END
GO

IF COL_LENGTH(N'identity.Users', N'Permission_MAX') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP CONSTRAINT IF EXISTS [DF_identity_Users_Permission_MAX];
    ALTER TABLE [identity].[Users] DROP COLUMN [Permission_MAX];
END
GO

IF COL_LENGTH(N'identity.Users', N'WorkFromHome') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP CONSTRAINT IF EXISTS [DF_identity_Users_WorkFromHome];
    ALTER TABLE [identity].[Users] DROP COLUMN [WorkFromHome];
END
GO

IF COL_LENGTH(N'identity.Users', N'WorkFromHome_MAX') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP CONSTRAINT IF EXISTS [DF_identity_Users_WorkFromHome_MAX];
    ALTER TABLE [identity].[Users] DROP COLUMN [WorkFromHome_MAX];
END
GO

IF COL_LENGTH(N'identity.Users', N'FromNextBalanceDaysUsed') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP CONSTRAINT IF EXISTS [DF_identity_Users_FromNextBalanceDaysUsed];
    ALTER TABLE [identity].[Users] DROP COLUMN [FromNextBalanceDaysUsed];
END
GO

IF COL_LENGTH(N'identity.Users', N'OldAnnualBalance') IS NOT NULL
BEGIN
    ALTER TABLE [identity].[Users] DROP CONSTRAINT IF EXISTS [DF_identity_Users_OldAnnualBalance];
    ALTER TABLE [identity].[Users] DROP COLUMN [OldAnnualBalance];
END
GO
