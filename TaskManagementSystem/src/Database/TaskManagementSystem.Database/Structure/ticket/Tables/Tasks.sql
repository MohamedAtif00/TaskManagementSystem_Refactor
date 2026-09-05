-- Source of truth for ticket.Tasks (TeamId replaces GroupId)
-- Deployed via Scripts/Migrations/006_ticket_Tables.sql

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
        CONSTRAINT [PK_ticket_Tasks] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO
