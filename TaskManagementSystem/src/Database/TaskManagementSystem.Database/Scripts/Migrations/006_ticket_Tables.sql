IF OBJECT_ID(N'ticket.Tasks', N'U') IS NULL
BEGIN
    CREATE TABLE [ticket].[Tasks]
    (
        [Id]                   INT           NOT NULL IDENTITY(1, 1),
        [Name]                 NVARCHAR(MAX) NOT NULL,
        [Status]               INT           NOT NULL,
        [Priority]             INT           NOT NULL,
        [Duration]             INT           NOT NULL,
        [CreatedAt]            DATETIME2     NOT NULL,
        [Pause]                BIT           NOT NULL CONSTRAINT [DF_ticket_Tasks_Pause] DEFAULT (0),
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
        CONSTRAINT [PK_ticket_Tasks] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ticket_Tasks_LearningObjectives_LearningObjectiveId]
            FOREIGN KEY ([LearningObjectiveId]) REFERENCES [curriculum].[LearningObjectives] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ticket_Tasks_Steps_StepId]
            FOREIGN KEY ([StepId]) REFERENCES [workflows].[Steps] ([Id]),
        CONSTRAINT [FK_ticket_Tasks_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]),
        CONSTRAINT [FK_ticket_Tasks_Teams_TeamId]
            FOREIGN KEY ([TeamId]) REFERENCES [organization].[Teams] ([Id]) ON DELETE SET NULL,
        CONSTRAINT [FK_ticket_Tasks_Tasks_FromId]
            FOREIGN KEY ([FromId]) REFERENCES [ticket].[Tasks] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_ticket_Tasks_LearningObjectiveId]
        ON [ticket].[Tasks] ([LearningObjectiveId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Tasks_StepId]
        ON [ticket].[Tasks] ([StepId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Tasks_UserId]
        ON [ticket].[Tasks] ([UserId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Tasks_TeamId]
        ON [ticket].[Tasks] ([TeamId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Tasks_FromId]
        ON [ticket].[Tasks] ([FromId] ASC);
END
GO

IF OBJECT_ID(N'ticket.TaskActivities', N'U') IS NULL
BEGIN
    CREATE TABLE [ticket].[TaskActivities]
    (
        [Id]              INT           NOT NULL IDENTITY(1, 1),
        [Type]            INT           NOT NULL,
        [TimeStamp]       DATETIME2     NOT NULL,
        [AdditionalInfo]  NVARCHAR(MAX) NULL,
        [TaskId]          INT           NOT NULL,
        [TaskSecondaryId] INT           NULL,
        [ActorOneId]      INT           NULL,
        [ActorTwoId]      INT           NULL,
        CONSTRAINT [PK_ticket_TaskActivities] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ticket_TaskActivities_Tasks_TaskId]
            FOREIGN KEY ([TaskId]) REFERENCES [ticket].[Tasks] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ticket_TaskActivities_Tasks_TaskSecondaryId]
            FOREIGN KEY ([TaskSecondaryId]) REFERENCES [ticket].[Tasks] ([Id]),
        CONSTRAINT [FK_ticket_TaskActivities_Users_ActorOneId]
            FOREIGN KEY ([ActorOneId]) REFERENCES [identity].[Users] ([Id]),
        CONSTRAINT [FK_ticket_TaskActivities_Users_ActorTwoId]
            FOREIGN KEY ([ActorTwoId]) REFERENCES [identity].[Users] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_ticket_TaskActivities_TaskId]
        ON [ticket].[TaskActivities] ([TaskId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_TaskActivities_TaskSecondaryId]
        ON [ticket].[TaskActivities] ([TaskSecondaryId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_TaskActivities_ActorOneId]
        ON [ticket].[TaskActivities] ([ActorOneId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_TaskActivities_ActorTwoId]
        ON [ticket].[TaskActivities] ([ActorTwoId] ASC);
END
GO

IF OBJECT_ID(N'ticket.TaskWorkTimes', N'U') IS NULL
BEGIN
    CREATE TABLE [ticket].[TaskWorkTimes]
    (
        [Id]         INT        NOT NULL IDENTITY(1, 1),
        [StartDate]  DATETIME2  NOT NULL,
        [EndDate]    DATETIME2  NULL,
        [Duration]   FLOAT      NOT NULL,
        [EndReason]  INT        NULL,
        [TaskId]     INT        NOT NULL,
        [UserId]     INT        NOT NULL,
        CONSTRAINT [PK_ticket_TaskWorkTimes] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ticket_TaskWorkTimes_Tasks_TaskId]
            FOREIGN KEY ([TaskId]) REFERENCES [ticket].[Tasks] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ticket_TaskWorkTimes_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_ticket_TaskWorkTimes_TaskId]
        ON [ticket].[TaskWorkTimes] ([TaskId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_TaskWorkTimes_UserId]
        ON [ticket].[TaskWorkTimes] ([UserId] ASC);
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
        [TaskId]              INT           NULL,
        [UserId]              INT           NOT NULL,
        [ChildId]             INT           NULL,
        CONSTRAINT [PK_ticket_Comments] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ticket_Comments_Comments_ChildId]
            FOREIGN KEY ([ChildId]) REFERENCES [ticket].[Comments] ([Id]),
        CONSTRAINT [FK_ticket_Comments_LearningObjectives_LearningObjectiveId]
            FOREIGN KEY ([LearningObjectiveId]) REFERENCES [curriculum].[LearningObjectives] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ticket_Comments_Tasks_TaskId]
            FOREIGN KEY ([TaskId]) REFERENCES [ticket].[Tasks] ([Id]),
        CONSTRAINT [FK_ticket_Comments_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_ticket_Comments_ChildId]
        ON [ticket].[Comments] ([ChildId] ASC)
        WHERE [ChildId] IS NOT NULL;

    CREATE NONCLUSTERED INDEX [IX_ticket_Comments_LearningObjectiveId]
        ON [ticket].[Comments] ([LearningObjectiveId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Comments_TaskId]
        ON [ticket].[Comments] ([TaskId] ASC);

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
        [TaskId]        INT           NOT NULL,
        [ToTaskId]      INT           NOT NULL,
        [UserId]        INT           NOT NULL,
        CONSTRAINT [PK_ticket_Rollbacks] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ticket_Rollbacks_Tasks_TaskId]
            FOREIGN KEY ([TaskId]) REFERENCES [ticket].[Tasks] ([Id]),
        CONSTRAINT [FK_ticket_Rollbacks_Tasks_ToTaskId]
            FOREIGN KEY ([ToTaskId]) REFERENCES [ticket].[Tasks] ([Id]),
        CONSTRAINT [FK_ticket_Rollbacks_Users_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_ticket_Rollbacks_TaskId]
        ON [ticket].[Rollbacks] ([TaskId] ASC);

    CREATE NONCLUSTERED INDEX [IX_ticket_Rollbacks_ToTaskId]
        ON [ticket].[Rollbacks] ([ToTaskId] ASC);

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
