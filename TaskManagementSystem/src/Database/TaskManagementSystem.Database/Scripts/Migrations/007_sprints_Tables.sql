IF OBJECT_ID(N'sprints.Sprints', N'U') IS NULL
BEGIN
    CREATE TABLE [sprints].[Sprints]
    (
        [Id]          INT           NOT NULL IDENTITY(1, 1),
        [Name]        NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [StartDate]   DATETIME2     NOT NULL,
        [EndDate]     DATETIME2     NOT NULL,
        [IsArchived]  BIT           NOT NULL CONSTRAINT [DF_sprints_Sprints_IsArchived] DEFAULT (0),
        CONSTRAINT [PK_sprints_Sprints] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF OBJECT_ID(N'sprints.SprintLearningObjectives', N'U') IS NULL
BEGIN
    CREATE TABLE [sprints].[SprintLearningObjectives]
    (
        [Id]                   INT NOT NULL IDENTITY(1, 1),
        [SprintId]             INT NOT NULL,
        [LearningObjectiveId]  INT NOT NULL,
        CONSTRAINT [PK_sprints_SprintLearningObjectives] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_sprints_SprintLearningObjectives_Sprints_SprintId]
            FOREIGN KEY ([SprintId]) REFERENCES [sprints].[Sprints] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_sprints_SprintLearningObjectives_LearningObjectives_LearningObjectiveId]
            FOREIGN KEY ([LearningObjectiveId]) REFERENCES [curriculum].[LearningObjectives] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_sprints_SprintLearningObjectives_SprintId]
        ON [sprints].[SprintLearningObjectives] ([SprintId] ASC);

    CREATE NONCLUSTERED INDEX [IX_sprints_SprintLearningObjectives_LearningObjectiveId]
        ON [sprints].[SprintLearningObjectives] ([LearningObjectiveId] ASC);
END
GO
