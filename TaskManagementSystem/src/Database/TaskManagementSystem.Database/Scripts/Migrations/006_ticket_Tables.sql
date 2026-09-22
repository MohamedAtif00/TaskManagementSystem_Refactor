IF OBJECT_ID(N'ticket.Tickets', N'U') IS NULL
BEGIN
    CREATE TABLE [ticket].[Tickets]
    (
        [Id]                   INT           NOT NULL IDENTITY(1, 1),
        [Name]                 NVARCHAR(MAX) NOT NULL,
        [Status]               INT           NOT NULL,
        [Priority]             INT           NOT NULL,
        [Duration]             INT           NOT NULL,
        [CreatedAt]            DATETIME2     NOT NULL,
        [Pause]                BIT           NOT NULL CONSTRAINT [DF_ticket_Tickets_Pause] DEFAULT (0),
        [Attention]            BIT           NOT NULL,
        [Flagged]              BIT           NOT NULL,
        [TL]                   BIT           NOT NULL,
        [IsReview]             BIT           NOT NULL,
        [IsRollback]           BIT           NOT NULL,
        [RollbackCount]        INT           NOT NULL,
        [Archived]             BIT           NOT NULL,
        [LearningObjectiveId]  INT           NOT NULL,
        [StepId]               INT           NULL,
        [UserId]               INT           NULL,
        [TeamId]               INT           NULL,
        [FromId]               INT           NULL,
        CONSTRAINT [PK_ticket_Tickets] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ticket_Tickets_LearningObjectives_LearningObjectiveId]
            FOREIGN KEY ([LearningObjectiveId]) REFERENCES [curriculum].[LearningObjectives] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ticket_Tickets_Steps_StepId]
            FOREIGN KEY ([StepId]) REFERENCES [workflows].[Steps] ([Id]),
        CONSTRAINT [FK_ticket_Tickets_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]),
        CONSTRAINT [FK_ticket_Tickets_Teams_TeamId]
            FOREIGN KEY ([TeamId]) REFERENCES [organization].[Teams] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_ticket_Tickets_Tickets_FromId]
            FOREIGN KEY ([FromId]) REFERENCES [ticket].[Tickets] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_ticket_Tickets_LearningObjectiveId]
        ON [ticket].[Tickets] ([LearningObjectiveId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Tickets_StepId]
        ON [ticket].[Tickets] ([StepId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Tickets_UserId]
        ON [ticket].[Tickets] ([UserId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Tickets_TeamId]
        ON [ticket].[Tickets] ([TeamId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Tickets_FromId]
        ON [ticket].[Tickets] ([FromId] ASC);
END
GO

IF OBJECT_ID(N'ticket.TicketActivities', N'U') IS NULL
BEGIN
    CREATE TABLE [ticket].[TicketActivities]
    (
        [Id]              INT           NOT NULL IDENTITY(1, 1),
        [Type]            INT           NOT NULL,
        [TimeStamp]       DATETIME2     NOT NULL,
        [AdditionalInfo]  NVARCHAR(MAX) NULL,
        [TicketId]          INT           NOT NULL,
        [TicketSecondaryId] INT           NULL,
        [ActorOneId]      INT           NULL,
        [ActorTwoId]      INT           NULL,
        CONSTRAINT [PK_ticket_TicketActivities] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ticket_TicketActivities_Tickets_TicketId]
            FOREIGN KEY ([TicketId]) REFERENCES [ticket].[Tickets] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ticket_TicketActivities_Tickets_TicketSecondaryId]
            FOREIGN KEY ([TicketSecondaryId]) REFERENCES [ticket].[Tickets] ([Id]),
        CONSTRAINT [FK_ticket_TicketActivities_Users_ActorOneId]
            FOREIGN KEY ([ActorOneId]) REFERENCES [identity].[Users] ([Id]),
        CONSTRAINT [FK_ticket_TicketActivities_Users_ActorTwoId]
            FOREIGN KEY ([ActorTwoId]) REFERENCES [identity].[Users] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_ticket_TicketActivities_TicketId]
        ON [ticket].[TicketActivities] ([TicketId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_TicketActivities_TicketSecondaryId]
        ON [ticket].[TicketActivities] ([TicketSecondaryId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_TicketActivities_ActorOneId]
        ON [ticket].[TicketActivities] ([ActorOneId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_TicketActivities_ActorTwoId]
        ON [ticket].[TicketActivities] ([ActorTwoId] ASC);
END
GO

IF OBJECT_ID(N'ticket.TicketWorkTimes', N'U') IS NULL
BEGIN
    CREATE TABLE [ticket].[TicketWorkTimes]
    (
        [Id]         INT        NOT NULL IDENTITY(1, 1),
        [StartDate]  DATETIME2  NOT NULL,
        [EndDate]    DATETIME2  NULL,
        [Duration]   FLOAT      NOT NULL,
        [EndReason]  INT        NULL,
        [TicketId]     INT        NOT NULL,
        [UserId]     INT        NOT NULL,
        CONSTRAINT [PK_ticket_TicketWorkTimes] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ticket_TicketWorkTimes_Tickets_TicketId]
            FOREIGN KEY ([TicketId]) REFERENCES [ticket].[Tickets] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ticket_TicketWorkTimes_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_ticket_TicketWorkTimes_TicketId]
        ON [ticket].[TicketWorkTimes] ([TicketId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_TicketWorkTimes_UserId]
        ON [ticket].[TicketWorkTimes] ([UserId] ASC);
END
GO

IF OBJECT_ID(N'ticket.Comments', N'U') IS NULL
BEGIN
    CREATE TABLE [ticket].[Comments]
    (
        [Id]                  INT           NOT NULL IDENTITY(1, 1),
        [Content]             NVARCHAR(MAX) NOT NULL,
        [CreatedAt]           DATETIME2     NOT NULL,
        [Timestamp]           DATETIME2     NOT NULL,
        [Archived]            BIT           NOT NULL,
        [LearningObjectiveId] INT           NOT NULL,
        [TicketId]              INT           NULL,
        [UserId]              INT           NOT NULL,
        [ChildId]             INT           NULL,
        CONSTRAINT [PK_ticket_Comments] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ticket_Comments_Comments_ChildId]
            FOREIGN KEY ([ChildId]) REFERENCES [ticket].[Comments] ([Id]),
        CONSTRAINT [FK_ticket_Comments_LearningObjectives_LearningObjectiveId]
            FOREIGN KEY ([LearningObjectiveId]) REFERENCES [curriculum].[LearningObjectives] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ticket_Comments_Tickets_TicketId]
            FOREIGN KEY ([TicketId]) REFERENCES [ticket].[Tickets] ([Id]),
        CONSTRAINT [FK_ticket_Comments_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_ticket_Comments_ChildId]
        ON [ticket].[Comments] ([ChildId] ASC)
        WHERE [ChildId] IS NOT NULL;

    CREATE NONCLUSTERED INDEX [IX_ticket_Comments_LearningObjectiveId]
        ON [ticket].[Comments] ([LearningObjectiveId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Comments_TicketId]
        ON [ticket].[Comments] ([TicketId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Comments_UserId]
        ON [ticket].[Comments] ([UserId] ASC);
END
GO

IF OBJECT_ID(N'ticket.Rollbacks', N'U') IS NULL
BEGIN
    CREATE TABLE [ticket].[Rollbacks]
    (
        [Id]            INT           NOT NULL IDENTITY(1, 1),
        [Clarification] NVARCHAR(MAX) NULL,
        [TicketId]        INT           NOT NULL,
        [ToTicketId]      INT           NOT NULL,
        [UserId]        INT           NOT NULL,
        CONSTRAINT [PK_ticket_Rollbacks] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ticket_Rollbacks_Tickets_TicketId]
            FOREIGN KEY ([TicketId]) REFERENCES [ticket].[Tickets] ([Id]),
        CONSTRAINT [FK_ticket_Rollbacks_Tickets_ToTicketId]
            FOREIGN KEY ([ToTicketId]) REFERENCES [ticket].[Tickets] ([Id]),
        CONSTRAINT [FK_ticket_Rollbacks_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_ticket_Rollbacks_TicketId]
        ON [ticket].[Rollbacks] ([TicketId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Rollbacks_ToTicketId]
        ON [ticket].[Rollbacks] ([ToTicketId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Rollbacks_UserId]
        ON [ticket].[Rollbacks] ([UserId] ASC);
END
GO

IF OBJECT_ID(N'ticket.RollbackIssues', N'U') IS NULL
BEGIN
    CREATE TABLE [ticket].[RollbackIssues]
    (
        [Id]         INT           NOT NULL IDENTITY(1, 1),
        [Note]       NVARCHAR(MAX) NULL,
        [RollbackId] INT           NOT NULL,
        [StepId]     INT           NOT NULL,
        CONSTRAINT [PK_ticket_RollbackIssues] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ticket_RollbackIssues_Rollbacks_RollbackId]
            FOREIGN KEY ([RollbackId]) REFERENCES [ticket].[Rollbacks] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ticket_RollbackIssues_Steps_StepId]
            FOREIGN KEY ([StepId]) REFERENCES [workflows].[Steps] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_ticket_RollbackIssues_RollbackId]
        ON [ticket].[RollbackIssues] ([RollbackId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_RollbackIssues_StepId]
        ON [ticket].[RollbackIssues] ([StepId] ASC);
END
GO
