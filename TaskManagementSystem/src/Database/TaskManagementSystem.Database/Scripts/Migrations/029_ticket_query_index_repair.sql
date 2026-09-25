-- Repair indexes skipped by 028 when same-named indexes already existed from 005/007.

IF EXISTS (
    SELECT 1
    FROM sys.indexes i
    WHERE i.name = N'IX_curriculum_Units_SubjectId'
      AND i.object_id = OBJECT_ID(N'curriculum.Units')
      AND NOT EXISTS (
          SELECT 1
          FROM sys.index_columns ic
          INNER JOIN sys.columns c
              ON c.object_id = ic.object_id
             AND c.column_id = ic.column_id
          WHERE ic.object_id = i.object_id
            AND ic.index_id = i.index_id
            AND ic.is_included_column = 1
            AND c.name = N'Archived'))
BEGIN
    DROP INDEX [IX_curriculum_Units_SubjectId] ON [curriculum].[Units];

    CREATE NONCLUSTERED INDEX [IX_curriculum_Units_SubjectId]
        ON [curriculum].[Units] ([SubjectId])
        INCLUDE ([Archived], [Name]);
END
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes i
    WHERE i.name = N'IX_sprints_SprintLearningObjectives_SprintId'
      AND i.object_id = OBJECT_ID(N'sprints.SprintLearningObjectives')
      AND NOT EXISTS (
          SELECT 1
          FROM sys.index_columns ic
          INNER JOIN sys.columns c
              ON c.object_id = ic.object_id
             AND c.column_id = ic.column_id
          WHERE ic.object_id = i.object_id
            AND ic.index_id = i.index_id
            AND ic.is_included_column = 1
            AND c.name = N'LearningObjectiveId'))
BEGIN
    DROP INDEX [IX_sprints_SprintLearningObjectives_SprintId] ON [sprints].[SprintLearningObjectives];

    CREATE NONCLUSTERED INDEX [IX_sprints_SprintLearningObjectives_SprintId]
        ON [sprints].[SprintLearningObjectives] ([SprintId])
        INCLUDE ([LearningObjectiveId]);
END
GO
