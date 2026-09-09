IF OBJECT_ID(N'hr.PublicHolidays', N'U') IS NULL
BEGIN
    CREATE TABLE [hr].[PublicHolidays]
    (
        [Id]              INT            NOT NULL IDENTITY(1, 1),
        [Name]            NVARCHAR(200)  NOT NULL,
        [Description]     NVARCHAR(1000) NULL,
        [StartDate]       DATE           NOT NULL,
        [EndDate]         DATE           NOT NULL,
        [CreatedAt]       DATETIME2      NOT NULL,
        [CreatedByUserId] INT            NOT NULL,
        CONSTRAINT [PK_hr_PublicHolidays] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_hr_PublicHolidays_StartDate_EndDate]
        ON [hr].[PublicHolidays] ([StartDate] ASC, [EndDate] ASC);
END
GO

IF COL_LENGTH('hr.LeaveRequests', 'WorkingDays') IS NULL
BEGIN
    ALTER TABLE [hr].[LeaveRequests]
        ADD [WorkingDays] INT NOT NULL CONSTRAINT [DF_hr_LeaveRequests_WorkingDays] DEFAULT (0);
END
GO
