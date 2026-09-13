-- Source of truth for hr.ForgotClockRequests
-- Deployed via Scripts/Migrations/016_hr_ForgotClockRequests.sql

IF OBJECT_ID(N'hr.ForgotClockRequests', N'U') IS NULL
BEGIN
    CREATE TABLE [hr].[ForgotClockRequests]
    (
        [Id]             INT           NOT NULL IDENTITY(1, 1),
        [PunchType]      NVARCHAR(MAX) NOT NULL,
        [Status]         NVARCHAR(MAX) NOT NULL,
        [AttendanceDate] DATETIME2     NOT NULL,
        [IntendedTime]   TIME          NOT NULL,
        [Reason]         NVARCHAR(MAX) NULL,
        [CreatedAt]      DATETIME2     NOT NULL,
        [UpdatedAt]      DATETIME2     NULL,
        [UserId]         INT           NOT NULL,
        [TeamleaderId]   INT           NULL,
        [SectionheadId]  INT           NULL,
        CONSTRAINT [PK_hr_ForgotClockRequests] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_hr_ForgotClockRequests_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_hr_ForgotClockRequests_UserId]
        ON [hr].[ForgotClockRequests] ([UserId] ASC);
END
GO
