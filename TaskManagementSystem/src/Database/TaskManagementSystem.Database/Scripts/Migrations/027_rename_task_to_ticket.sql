-- Rename business "Task" tables/columns to "Ticket" (avoids .NET Task naming collision in code).
-- Idempotent: skips when ticket.Tickets already exists.

IF OBJECT_ID(N'ticket.Tasks', N'U') IS NOT NULL AND OBJECT_ID(N'ticket.Tickets', N'U') IS NULL
BEGIN
    IF OBJECT_ID(N'ticket.FK_ticket_TaskActivities_Tasks_TaskId', N'F') IS NOT NULL
        ALTER TABLE [ticket].[TaskActivities] DROP CONSTRAINT [FK_ticket_TaskActivities_Tasks_TaskId];
    IF OBJECT_ID(N'ticket.FK_ticket_TaskActivities_Tasks_TaskSecondaryId', N'F') IS NOT NULL
        ALTER TABLE [ticket].[TaskActivities] DROP CONSTRAINT [FK_ticket_TaskActivities_Tasks_TaskSecondaryId];
    IF OBJECT_ID(N'ticket.FK_ticket_TaskWorkTimes_Tasks_TaskId', N'F') IS NOT NULL
        ALTER TABLE [ticket].[TaskWorkTimes] DROP CONSTRAINT [FK_ticket_TaskWorkTimes_Tasks_TaskId];
    IF OBJECT_ID(N'ticket.FK_ticket_Comments_Tasks_TaskId', N'F') IS NOT NULL
        ALTER TABLE [ticket].[Comments] DROP CONSTRAINT [FK_ticket_Comments_Tasks_TaskId];
    IF OBJECT_ID(N'ticket.FK_ticket_Rollbacks_Tasks_TaskId', N'F') IS NOT NULL
        ALTER TABLE [ticket].[Rollbacks] DROP CONSTRAINT [FK_ticket_Rollbacks_Tasks_TaskId];
    IF OBJECT_ID(N'ticket.FK_ticket_Rollbacks_Tasks_ToTaskId', N'F') IS NOT NULL
        ALTER TABLE [ticket].[Rollbacks] DROP CONSTRAINT [FK_ticket_Rollbacks_Tasks_ToTaskId];
    IF OBJECT_ID(N'ticket.FK_ticket_Tasks_Tasks_FromId', N'F') IS NOT NULL
        ALTER TABLE [ticket].[Tasks] DROP CONSTRAINT [FK_ticket_Tasks_Tasks_FromId];

    EXEC sp_rename N'ticket.Tasks', N'Tickets', N'OBJECT';
    EXEC sp_rename N'ticket.TaskActivities', N'TicketActivities', N'OBJECT';
    EXEC sp_rename N'ticket.TaskWorkTimes', N'TicketWorkTimes', N'OBJECT';

    EXEC sp_rename N'ticket.TicketActivities.TaskId', N'TicketId', N'COLUMN';
    EXEC sp_rename N'ticket.TicketActivities.TaskSecondaryId', N'TicketSecondaryId', N'COLUMN';
    EXEC sp_rename N'ticket.TicketWorkTimes.TaskId', N'TicketId', N'COLUMN';
    EXEC sp_rename N'ticket.Comments.TaskId', N'TicketId', N'COLUMN';
    EXEC sp_rename N'ticket.Rollbacks.TaskId', N'TicketId', N'COLUMN';
    EXEC sp_rename N'ticket.Rollbacks.ToTaskId', N'ToTicketId', N'COLUMN';

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ticket_TaskActivities_TaskId' AND object_id = OBJECT_ID(N'ticket.TicketActivities'))
        EXEC sp_rename N'ticket.TicketActivities.IX_ticket_TaskActivities_TaskId', N'IX_ticket_TicketActivities_TicketId', N'INDEX';
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ticket_TaskActivities_TaskSecondaryId' AND object_id = OBJECT_ID(N'ticket.TicketActivities'))
        EXEC sp_rename N'ticket.TicketActivities.IX_ticket_TaskActivities_TaskSecondaryId', N'IX_ticket_TicketActivities_TicketSecondaryId', N'INDEX';
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ticket_TaskWorkTimes_TaskId' AND object_id = OBJECT_ID(N'ticket.TicketWorkTimes'))
        EXEC sp_rename N'ticket.TicketWorkTimes.IX_ticket_TaskWorkTimes_TaskId', N'IX_ticket_TicketWorkTimes_TicketId', N'INDEX';
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ticket_Comments_TaskId' AND object_id = OBJECT_ID(N'ticket.Comments'))
        EXEC sp_rename N'ticket.Comments.IX_ticket_Comments_TaskId', N'IX_ticket_Comments_TicketId', N'INDEX';
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ticket_Rollbacks_TaskId' AND object_id = OBJECT_ID(N'ticket.Rollbacks'))
        EXEC sp_rename N'ticket.Rollbacks.IX_ticket_Rollbacks_TaskId', N'IX_ticket_Rollbacks_TicketId', N'INDEX';
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ticket_Rollbacks_ToTaskId' AND object_id = OBJECT_ID(N'ticket.Rollbacks'))
        EXEC sp_rename N'ticket.Rollbacks.IX_ticket_Rollbacks_ToTaskId', N'IX_ticket_Rollbacks_ToTicketId', N'INDEX';

    EXEC sp_rename N'ticket.PK_ticket_Tasks', N'PK_ticket_Tickets', N'OBJECT';
    EXEC sp_rename N'ticket.PK_ticket_TaskActivities', N'PK_ticket_TicketActivities', N'OBJECT';
    EXEC sp_rename N'ticket.PK_ticket_TaskWorkTimes', N'PK_ticket_TicketWorkTimes', N'OBJECT';

    ALTER TABLE [ticket].[Tickets] ADD CONSTRAINT [FK_ticket_Tickets_Tickets_FromId]
        FOREIGN KEY ([FromId]) REFERENCES [ticket].[Tickets] ([Id]);

    ALTER TABLE [ticket].[TicketActivities] ADD CONSTRAINT [FK_ticket_TicketActivities_Tickets_TicketId]
        FOREIGN KEY ([TicketId]) REFERENCES [ticket].[Tickets] ([Id]) ON DELETE CASCADE;
    ALTER TABLE [ticket].[TicketActivities] ADD CONSTRAINT [FK_ticket_TicketActivities_Tickets_TicketSecondaryId]
        FOREIGN KEY ([TicketSecondaryId]) REFERENCES [ticket].[Tickets] ([Id]);

    ALTER TABLE [ticket].[TicketWorkTimes] ADD CONSTRAINT [FK_ticket_TicketWorkTimes_Tickets_TicketId]
        FOREIGN KEY ([TicketId]) REFERENCES [ticket].[Tickets] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [ticket].[Comments] ADD CONSTRAINT [FK_ticket_Comments_Tickets_TicketId]
        FOREIGN KEY ([TicketId]) REFERENCES [ticket].[Tickets] ([Id]);

    ALTER TABLE [ticket].[Rollbacks] ADD CONSTRAINT [FK_ticket_Rollbacks_Tickets_TicketId]
        FOREIGN KEY ([TicketId]) REFERENCES [ticket].[Tickets] ([Id]);
    ALTER TABLE [ticket].[Rollbacks] ADD CONSTRAINT [FK_ticket_Rollbacks_Tickets_ToTicketId]
        FOREIGN KEY ([ToTicketId]) REFERENCES [ticket].[Tickets] ([Id]);
END
GO

IF OBJECT_ID(N'workflows.TaskBank', N'U') IS NOT NULL AND OBJECT_ID(N'workflows.TicketBank', N'U') IS NULL
BEGIN
    IF OBJECT_ID(N'workflows.FK_workflows_Steps_TaskBank_TaskBankId', N'F') IS NOT NULL
        ALTER TABLE [workflows].[Steps] DROP CONSTRAINT [FK_workflows_Steps_TaskBank_TaskBankId];

    EXEC sp_rename N'workflows.TaskBank', N'TicketBank', N'OBJECT';
    EXEC sp_rename N'workflows.Steps.TaskBankId', N'TicketBankId', N'COLUMN';

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_workflows_Steps_TaskBankId' AND object_id = OBJECT_ID(N'workflows.Steps'))
        EXEC sp_rename N'workflows.Steps.IX_workflows_Steps_TaskBankId', N'IX_workflows_Steps_TicketBankId', N'INDEX';
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_workflows_TaskBank_TeamId' AND object_id = OBJECT_ID(N'workflows.TicketBank'))
        EXEC sp_rename N'workflows.TicketBank.IX_workflows_TaskBank_TeamId', N'IX_workflows_TicketBank_TeamId', N'INDEX';

    EXEC sp_rename N'workflows.PK_workflows_TaskBank', N'PK_workflows_TicketBank', N'OBJECT';

    ALTER TABLE [workflows].[Steps] ADD CONSTRAINT [FK_workflows_Steps_TicketBank_TicketBankId]
        FOREIGN KEY ([TicketBankId]) REFERENCES [workflows].[TicketBank] ([Id]) ON DELETE CASCADE;
END
GO
