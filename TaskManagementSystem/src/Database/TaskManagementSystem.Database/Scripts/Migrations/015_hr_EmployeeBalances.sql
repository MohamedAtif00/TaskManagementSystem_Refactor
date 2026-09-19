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

IF NOT EXISTS (SELECT 1 FROM [hr].[EmployeeBalances])
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
        NULL,
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
    FROM [identity].[Users] AS u;
END
GO
