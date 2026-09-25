-- Speed up ticket list/stats queries on large legacy datasets.

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ticket_Tickets_Archived_LearningObjectiveId'
      AND object_id = OBJECT_ID(N'ticket.Tickets'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ticket_Tickets_Archived_LearningObjectiveId]
        ON [ticket].[Tickets] ([Archived], [LearningObjectiveId])
        INCLUDE ([Status], [UserId], [CreatedAt], [Name], [Priority], [Duration], [StepId], [TeamId], [Pause], [Attention], [Flagged], [IsRollback], [RollbackCount]);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_curriculum_LearningObjectives_LessonId_Archived'
      AND object_id = OBJECT_ID(N'curriculum.LearningObjectives'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_curriculum_LearningObjectives_LessonId_Archived]
        ON [curriculum].[LearningObjectives] ([LessonId], [Archived])
        INCLUDE ([Name], [Tag], [Template], [Environment], [SchemaId]);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_curriculum_Units_SubjectId'
      AND object_id = OBJECT_ID(N'curriculum.Units'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_curriculum_Units_SubjectId]
        ON [curriculum].[Units] ([SubjectId])
        INCLUDE ([Archived], [Name]);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_sprints_SprintLearningObjectives_SprintId'
      AND object_id = OBJECT_ID(N'sprints.SprintLearningObjectives'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_sprints_SprintLearningObjectives_SprintId]
        ON [sprints].[SprintLearningObjectives] ([SprintId])
        INCLUDE ([LearningObjectiveId]);
END
GO
