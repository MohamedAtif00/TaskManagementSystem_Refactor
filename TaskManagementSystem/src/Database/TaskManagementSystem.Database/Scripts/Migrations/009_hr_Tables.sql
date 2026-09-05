IF OBJECT_ID(N'hr.LeaveRequests', N'U') IS NULL
BEGIN
    CREATE TABLE [hr].[LeaveRequests]
    (
        [Id]                         INT           NOT NULL IDENTITY(1, 1),
        [Type]                       NVARCHAR(MAX) NOT NULL,
        [Status]                     NVARCHAR(MAX) NOT NULL,
        [StartDate]                  DATETIME2     NOT NULL,
        [EndDate]                    DATETIME2     NOT NULL,
        [Reason]                     NVARCHAR(MAX) NULL,
        [NoteForManager]             NVARCHAR(MAX) NULL,
        [MedicalCertificateFileName] NVARCHAR(MAX) NULL,
        [MedicalCertificatePath]     NVARCHAR(MAX) NULL,
        [DateCreated]                DATETIME2     NULL,
        [UserId]                     INT           NOT NULL,
        [TeamleaderId]               INT           NULL,
        [SectionheadId]              INT           NULL,
        CONSTRAINT [PK_hr_LeaveRequests] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_hr_LeaveRequests_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_hr_LeaveRequests_UserId]
        ON [hr].[LeaveRequests] ([UserId] ASC);
END
GO

IF OBJECT_ID(N'hr.LeaveResetLogs', N'U') IS NULL
BEGIN
    CREATE TABLE [hr].[LeaveResetLogs]
    (
        [Id]         INT       NOT NULL IDENTITY(1, 1),
        [Year]       INT       NOT NULL,
        [ExecutedAt] DATETIME2 NOT NULL,
        CONSTRAINT [PK_hr_LeaveResetLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF OBJECT_ID(N'hr.Permissions', N'U') IS NULL
BEGIN
    CREATE TABLE [hr].[Permissions]
    (
        [Id]             INT           NOT NULL IDENTITY(1, 1),
        [Type]           NVARCHAR(MAX) NOT NULL,
        [Status]         NVARCHAR(MAX) NOT NULL,
        [PermissionDate] DATETIME2     NOT NULL,
        [FromTime]       TIME          NOT NULL,
        [ToTime]         TIME          NOT NULL,
        [Reason]         NVARCHAR(MAX) NULL,
        [CreatedAt]      DATETIME2     NOT NULL,
        [UpdatedAt]      DATETIME2     NULL,
        [UserId]         INT           NOT NULL,
        [TeamleaderId]   INT           NULL,
        [SectionheadId]  INT           NULL,
        CONSTRAINT [PK_hr_Permissions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_hr_Permissions_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_hr_Permissions_UserId]
        ON [hr].[Permissions] ([UserId] ASC);
END
GO

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

IF OBJECT_ID(N'hr.Opinions', N'U') IS NULL
BEGIN
    CREATE TABLE [hr].[Opinions]
    (
        [Id]                    INT           NOT NULL IDENTITY(1, 1),
        [Comment]               NVARCHAR(MAX) NULL,
        [IsApproved]            BIT           NOT NULL,
        [CreatedAt]             DATETIME2     NOT NULL,
        [UserId]                INT           NOT NULL,
        [LeaveRequestId]        INT           NULL,
        [PermissionId]          INT           NULL,
        [WorkFromHomeRequestId] INT           NULL,
        CONSTRAINT [PK_hr_Opinions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_hr_Opinions_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_hr_Opinions_LeaveRequests_LeaveRequestId]
            FOREIGN KEY ([LeaveRequestId]) REFERENCES [hr].[LeaveRequests] ([Id]),
        CONSTRAINT [FK_hr_Opinions_Permissions_PermissionId]
            FOREIGN KEY ([PermissionId]) REFERENCES [hr].[Permissions] ([Id]),
        CONSTRAINT [FK_hr_Opinions_WorkFromHomeRequests_WorkFromHomeRequestId]
            FOREIGN KEY ([WorkFromHomeRequestId]) REFERENCES [hr].[WorkFromHomeRequests] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_hr_Opinions_UserId]
        ON [hr].[Opinions] ([UserId] ASC);

    CREATE NONCLUSTERED INDEX [IX_hr_Opinions_LeaveRequestId]
        ON [hr].[Opinions] ([LeaveRequestId] ASC);

    CREATE NONCLUSTERED INDEX [IX_hr_Opinions_PermissionId]
        ON [hr].[Opinions] ([PermissionId] ASC);

    CREATE NONCLUSTERED INDEX [IX_hr_Opinions_WorkFromHomeRequestId]
        ON [hr].[Opinions] ([WorkFromHomeRequestId] ASC);
END
GO
