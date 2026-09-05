IF OBJECT_ID(N'workflows.SchemaTypes', N'U') IS NULL
BEGIN
    CREATE TABLE [workflows].[SchemaTypes]
    (
        [Id]          INT           NOT NULL IDENTITY(1, 1),
        [Name]        NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        CONSTRAINT [PK_workflows_SchemaTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

IF OBJECT_ID(N'workflows.Schemas', N'U') IS NULL
BEGIN
    CREATE TABLE [workflows].[Schemas]
    (
        [Id]          INT           NOT NULL IDENTITY(1, 1),
        [Name]        NVARCHAR(MAX) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [Archived]    BIT           NOT NULL,
        [TypeId]      INT           NULL,
        CONSTRAINT [PK_workflows_Schemas] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_workflows_Schemas_SchemaTypes_TypeId]
            FOREIGN KEY ([TypeId]) REFERENCES [workflows].[SchemaTypes] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_workflows_Schemas_TypeId]
        ON [workflows].[Schemas] ([TypeId] ASC);
END
GO

IF OBJECT_ID(N'workflows.TaskBank', N'U') IS NULL
BEGIN
    CREATE TABLE [workflows].[TaskBank]
    (
        [Id]       INT           NOT NULL IDENTITY(1, 1),
        [Name]     NVARCHAR(MAX) NOT NULL,
        [Duration] INT           NOT NULL,
        [Type]     INT           NOT NULL,
        [Active]   BIT           NOT NULL,
        [TL]       BIT           NOT NULL,
        [TeamId]   INT           NOT NULL,
        CONSTRAINT [PK_workflows_TaskBank] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_workflows_TaskBank_Teams_TeamId]
            FOREIGN KEY ([TeamId]) REFERENCES [organization].[Teams] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_workflows_TaskBank_TeamId]
        ON [workflows].[TaskBank] ([TeamId] ASC);
END
GO

IF OBJECT_ID(N'workflows.Nodes', N'U') IS NULL
BEGIN
    CREATE TABLE [workflows].[Nodes]
    (
        [Id]       INT           NOT NULL IDENTITY(1, 1),
        [Name]     NVARCHAR(MAX) NOT NULL,
        [Order]    INT           NOT NULL,
        [isStart]  BIT           NOT NULL,
        [isEnd]    BIT           NOT NULL,
        [Archived] BIT           NOT NULL CONSTRAINT [DF_workflows_Nodes_Archived] DEFAULT (0),
        [SchemaId] INT           NOT NULL,
        CONSTRAINT [PK_workflows_Nodes] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_workflows_Nodes_Schemas_SchemaId]
            FOREIGN KEY ([SchemaId]) REFERENCES [workflows].[Schemas] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_workflows_Nodes_SchemaId]
        ON [workflows].[Nodes] ([SchemaId] ASC);
END
GO

IF OBJECT_ID(N'workflows.Steps', N'U') IS NULL
BEGIN
    CREATE TABLE [workflows].[Steps]
    (
        [Id]         INT NOT NULL IDENTITY(1, 1),
        [Order]      INT NOT NULL,
        [Duration]   INT NOT NULL,
        [Priority]   INT NOT NULL,
        [Archived]   BIT NOT NULL,
        [NodeId]     INT NOT NULL,
        [TaskBankId] INT NOT NULL,
        CONSTRAINT [PK_workflows_Steps] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_workflows_Steps_Nodes_NodeId]
            FOREIGN KEY ([NodeId]) REFERENCES [workflows].[Nodes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_workflows_Steps_TaskBank_TaskBankId]
            FOREIGN KEY ([TaskBankId]) REFERENCES [workflows].[TaskBank] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_workflows_Steps_NodeId]
        ON [workflows].[Steps] ([NodeId] ASC);

    CREATE NONCLUSTERED INDEX [IX_workflows_Steps_TaskBankId]
        ON [workflows].[Steps] ([TaskBankId] ASC);
END
GO

IF OBJECT_ID(N'workflows.NodeSequences', N'U') IS NULL
BEGIN
    CREATE TABLE [workflows].[NodeSequences]
    (
        [NextId]     INT NOT NULL,
        [PreviousId] INT NOT NULL,
        CONSTRAINT [PK_workflows_NodeSequences] PRIMARY KEY CLUSTERED ([NextId] ASC, [PreviousId] ASC),
        CONSTRAINT [FK_workflows_NodeSequences_Nodes_NextId]
            FOREIGN KEY ([NextId]) REFERENCES [workflows].[Nodes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_workflows_NodeSequences_Nodes_PreviousId]
            FOREIGN KEY ([PreviousId]) REFERENCES [workflows].[Nodes] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_workflows_NodeSequences_PreviousId]
        ON [workflows].[NodeSequences] ([PreviousId] ASC);
END
GO

IF OBJECT_ID(N'workflows.RSteps', N'U') IS NULL
BEGIN
    CREATE TABLE [workflows].[RSteps]
    (
        [FromId]      INT NOT NULL,
        [RollbacksId] INT NOT NULL,
        CONSTRAINT [PK_workflows_RSteps] PRIMARY KEY CLUSTERED ([FromId] ASC, [RollbacksId] ASC),
        CONSTRAINT [FK_workflows_RSteps_Steps_FromId]
            FOREIGN KEY ([FromId]) REFERENCES [workflows].[Steps] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_workflows_RSteps_Steps_RollbacksId]
            FOREIGN KEY ([RollbacksId]) REFERENCES [workflows].[Steps] ([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_workflows_RSteps_RollbacksId]
        ON [workflows].[RSteps] ([RollbacksId] ASC);
END
GO
