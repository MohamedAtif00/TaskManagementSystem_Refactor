-- Source of truth for workflows.TicketBank (TeamId replaces GroupId)
-- Deployed via Scripts/Migrations/004_workflows_Tables.sql + 027_rename_task_to_ticket.sql

IF OBJECT_ID(N'workflows.TicketBank', N'U') IS NULL
BEGIN
    CREATE TABLE [workflows].[TicketBank]
    (
        [Id]       INT           NOT NULL IDENTITY(1, 1),
        [Name]     NVARCHAR(MAX) NOT NULL,
        [Duration] INT           NOT NULL,
        [Type]     INT           NOT NULL,
        [Active]   BIT           NOT NULL,
        [TL]       BIT           NOT NULL,
        [TeamId]   INT           NOT NULL,
        CONSTRAINT [PK_workflows_TicketBank] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO
