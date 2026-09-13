-- Source of truth for hr.WorkFromHomeRequests
-- Deployed via Scripts/Migrations/009_hr_Tables.sql

IF OBJECT_ID(N'hr.WorkFromHomeRequests', N'U') IS NULL
BEGIN
    CREATE TABLE [hr].[WorkFromHomeRequests]
    (
        [Id]             INT           NOT NULL IDENTITY(1, 1),
        [Date]           DATETIME2     NOT NULL,
        [DateCreated]    DATETIME2     NULL,
        [NoteForManager] NVARCHAR(MAX) NULL,
        [Status]         INT           NOT NULL,
        [UserId]         INT           NOT NULL,
        [TeamleaderId]   INT           NULL,
        [SectionheadId]  INT           NULL,
        CONSTRAINT [PK_hr_WorkFromHomeRequests] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_hr_WorkFromHomeRequests_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_hr_WorkFromHomeRequests_UserId]
        ON [hr].[WorkFromHomeRequests] ([UserId] ASC);
END
GO
