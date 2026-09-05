-- Source of truth for workflows.TaskBank (TeamId replaces GroupId)
-- Deployed via Scripts/Migrations/004_workflows_Tables.sql

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
        CONSTRAINT [PK_workflows_TaskBank] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO
