-- Source of truth for hr.EmployeeBalances
-- Deployed via Scripts/Migrations/015_hr_EmployeeBalances.sql

IF OBJECT_ID(N'hr.EmployeeBalances', N'U') IS NULL
BEGIN
    CREATE TABLE [hr].[EmployeeBalances]
    (
        [UserId]                  INT NOT NULL,
        [TeamId]                  INT NULL,
        [TeamleaderId]            INT NULL,
        [Role]                    INT NOT NULL,
        [AnnualLeave]             INT NOT NULL,
        [AnnualLeaveMax]          INT NOT NULL,
        [EmergencyLeave]          INT NOT NULL,
        [EmergencyLeaveMax]       INT NOT NULL,
        [SickLeave]               INT NOT NULL,
        [Permission]              INT NOT NULL,
        [PermissionMax]           INT NOT NULL,
        [WorkFromHome]            INT NOT NULL,
        [WorkFromHomeMax]         INT NOT NULL,
        [FromNextBalanceDaysUsed] INT NOT NULL,
        [OldAnnualBalance]        INT NOT NULL,
        CONSTRAINT [PK_hr_EmployeeBalances] PRIMARY KEY CLUSTERED ([UserId] ASC),
        CONSTRAINT [FK_hr_EmployeeBalances_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );
END
GO
