-- Test harness for the ReplaceFoldersWithCurriculumHierarchy porting logic.
-- Builds a scratch DB with a mixed-shape legacy Folders tree and runs the
-- same SQL as the migration, then prints the resulting hierarchy.

IF DB_ID(N'CurriculumMigTest') IS NOT NULL
BEGIN
    ALTER DATABASE CurriculumMigTest SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CurriculumMigTest;
END;
CREATE DATABASE CurriculumMigTest;
GO
USE CurriculumMigTest;
GO
CREATE TABLE Folders (Id INT IDENTITY PRIMARY KEY, ParentFolderId INT NULL, Name NVARCHAR(400) NOT NULL);
CREATE TABLE Subjects (Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(400) NOT NULL, FolderId INT NOT NULL);

-- Mixed-shape data under one root:
--   'Term 1'          legacy term  (children, no grandchildren)
--   'Term 2'          legacy term
--   'new project'     real project (has grandchildren)
--   'Selah Eltelmeez' childless project
INSERT INTO Folders (ParentFolderId, Name) VALUES (NULL, N'2026/2027');   -- 1
INSERT INTO Folders (ParentFolderId, Name) VALUES (1, N'Term 1');         -- 2
INSERT INTO Folders (ParentFolderId, Name) VALUES (1, N'Term 2');         -- 3
INSERT INTO Folders (ParentFolderId, Name) VALUES (1, N'new project');    -- 4
INSERT INTO Folders (ParentFolderId, Name) VALUES (1, N'Selah Eltelmeez');-- 5
INSERT INTO Folders (ParentFolderId, Name) VALUES (2, N'Arabic');         -- 6
INSERT INTO Folders (ParentFolderId, Name) VALUES (2, N'Math');           -- 7
INSERT INTO Folders (ParentFolderId, Name) VALUES (3, N'Arabic');         -- 8
INSERT INTO Folders (ParentFolderId, Name) VALUES (4, N'term 1');         -- 9
INSERT INTO Folders (ParentFolderId, Name) VALUES (9, N'Math');           -- 10

INSERT INTO Subjects (Name, FolderId) VALUES
    (N'ara_1r_1a', 6),
    (N'mth_1r_1a', 7),
    (N'ara_1r_2a', 8),
    (N'math x', 10);
GO

-- ===== Migration block 1: create hierarchy tables =====
CREATE TABLE [AcademicYears] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK_AcademicYears] PRIMARY KEY ([Id])
);
CREATE TABLE [CurriculumProjects] (
    [Id] int NOT NULL IDENTITY,
    [YearId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK_CurriculumProjects] PRIMARY KEY ([Id])
);
CREATE TABLE [CurriculumTerms] (
    [Id] int NOT NULL IDENTITY,
    [ProjectId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [StartDate] datetime2 NULL,
    [EndDate] datetime2 NULL,
    CONSTRAINT [PK_CurriculumTerms] PRIMARY KEY ([Id])
);
CREATE TABLE [SubjectGroups] (
    [Id] int NOT NULL IDENTITY,
    [TermId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_SubjectGroups] PRIMARY KEY ([Id])
);
GO

-- ===== Migration block 2: add SubjectGroupId =====
ALTER TABLE [Subjects] ADD [SubjectGroupId] INT NULL;
GO

-- ===== Migration block 3: port data (same SQL as the migration) =====
IF OBJECT_ID(N'dbo.Folders', N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'tempdb..#FolderDepth') IS NOT NULL DROP TABLE #FolderDepth;

    ;WITH FolderTree AS (
        SELECT f.[Id], f.[ParentFolderId], 0 AS [Depth], f.[Id] AS [RootId]
        FROM [Folders] f
        WHERE f.[ParentFolderId] IS NULL
        UNION ALL
        SELECT c.[Id], c.[ParentFolderId], ft.[Depth] + 1, ft.[RootId]
        FROM [Folders] c
        INNER JOIN FolderTree ft ON c.[ParentFolderId] = ft.[Id]
    )
    SELECT ft.[Id], ft.[Depth], ft.[RootId], f.[Name], f.[ParentFolderId]
    INTO #FolderDepth
    FROM FolderTree ft
    INNER JOIN [Folders] f ON f.[Id] = ft.[Id];

    DECLARE @map TABLE (FolderId INT PRIMARY KEY, EntityType NVARCHAR(20), NewId INT);

    INSERT INTO [AcademicYears] ([Name])
    SELECT fd.[Name]
    FROM #FolderDepth fd
    WHERE fd.[Depth] = 0
      AND NOT EXISTS (SELECT 1 FROM [AcademicYears] ay WHERE ay.[Name] = fd.[Name]);

    INSERT INTO @map (FolderId, EntityType, NewId)
    SELECT fd.[Id], N'year', MIN(ay.[Id])
    FROM #FolderDepth fd
    INNER JOIN [AcademicYears] ay ON ay.[Name] = fd.[Name]
    WHERE fd.[Depth] = 0
      AND NOT EXISTS (SELECT 1 FROM @map m WHERE m.FolderId = fd.[Id])
    GROUP BY fd.[Id];

    DECLARE @depth1 TABLE (Id INT PRIMARY KEY, RootId INT, IsProject BIT);
    INSERT INTO @depth1 (Id, RootId, IsProject)
    SELECT fd.[Id], fd.[RootId],
           CASE
               WHEN EXISTS (
                   SELECT 1
                   FROM #FolderDepth c
                   INNER JOIN #FolderDepth g ON g.[ParentFolderId] = c.[Id]
                   WHERE c.[ParentFolderId] = fd.[Id]) THEN 1
               WHEN NOT EXISTS (
                   SELECT 1 FROM #FolderDepth c WHERE c.[ParentFolderId] = fd.[Id]) THEN 1
               ELSE 0
           END
    FROM #FolderDepth fd
    WHERE fd.[Depth] = 1;

    INSERT INTO [CurriculumProjects] ([YearId], [Name])
    SELECT ym.[NewId], fd.[Name]
    FROM #FolderDepth fd
    INNER JOIN @depth1 d1 ON d1.[Id] = fd.[Id] AND d1.[IsProject] = 1
    INNER JOIN @map ym ON ym.[FolderId] = fd.[RootId] AND ym.[EntityType] = N'year'
    WHERE NOT EXISTS (
          SELECT 1 FROM [CurriculumProjects] cp
          WHERE cp.[YearId] = ym.[NewId] AND cp.[Name] = fd.[Name]);

    INSERT INTO @map (FolderId, EntityType, NewId)
    SELECT fd.[Id], N'project', MIN(cp.[Id])
    FROM #FolderDepth fd
    INNER JOIN @depth1 d1 ON d1.[Id] = fd.[Id] AND d1.[IsProject] = 1
    INNER JOIN @map ym ON ym.[FolderId] = fd.[RootId] AND ym.[EntityType] = N'year'
    INNER JOIN [CurriculumProjects] cp ON cp.[YearId] = ym.[NewId] AND cp.[Name] = fd.[Name]
    WHERE NOT EXISTS (SELECT 1 FROM @map m WHERE m.FolderId = fd.[Id])
    GROUP BY fd.[Id];

    INSERT INTO [CurriculumProjects] ([YearId], [Name])
    SELECT ym.[NewId], fd.[Name]
    FROM #FolderDepth fd
    INNER JOIN @map ym ON ym.[FolderId] = fd.[Id] AND ym.[EntityType] = N'year'
    WHERE fd.[Depth] = 0
      AND EXISTS (SELECT 1 FROM @depth1 d1 WHERE d1.[RootId] = fd.[Id] AND d1.[IsProject] = 0)
      AND NOT EXISTS (SELECT 1 FROM [CurriculumProjects] cp WHERE cp.[YearId] = ym.[NewId]);

    DECLARE @legacyProject TABLE (RootId INT PRIMARY KEY, ProjectId INT);
    INSERT INTO @legacyProject (RootId, ProjectId)
    SELECT fd.[Id],
        COALESCE(
            (SELECT MIN(cp.[Id])
             FROM [CurriculumProjects] cp
             INNER JOIN @map pm ON pm.[EntityType] = N'project' AND pm.[NewId] = cp.[Id]
             INNER JOIN @depth1 d1 ON d1.[Id] = pm.[FolderId] AND d1.[IsProject] = 1 AND d1.[RootId] = fd.[Id]
             WHERE cp.[YearId] = ym.[NewId]
               AND NOT EXISTS (
                   SELECT 1 FROM #FolderDepth c WHERE c.[ParentFolderId] = pm.[FolderId])),
            (SELECT MIN(cp.[Id])
             FROM [CurriculumProjects] cp
             INNER JOIN @map pm ON pm.[EntityType] = N'project' AND pm.[NewId] = cp.[Id]
             INNER JOIN @depth1 d1 ON d1.[Id] = pm.[FolderId] AND d1.[IsProject] = 1 AND d1.[RootId] = fd.[Id]
             WHERE cp.[YearId] = ym.[NewId]),
            (SELECT MIN(cp.[Id]) FROM [CurriculumProjects] cp WHERE cp.[YearId] = ym.[NewId]))
    FROM #FolderDepth fd
    INNER JOIN @map ym ON ym.[FolderId] = fd.[Id] AND ym.[EntityType] = N'year'
    WHERE fd.[Depth] = 0
      AND EXISTS (SELECT 1 FROM @depth1 d1 WHERE d1.[RootId] = fd.[Id] AND d1.[IsProject] = 0)
    GROUP BY fd.[Id], ym.[NewId];

    INSERT INTO [CurriculumTerms] ([ProjectId], [Name])
    SELECT pm.[NewId], fd.[Name]
    FROM #FolderDepth fd
    INNER JOIN @map pm ON pm.[FolderId] = fd.[ParentFolderId] AND pm.[EntityType] = N'project'
    WHERE fd.[Depth] = 2
      AND NOT EXISTS (
          SELECT 1 FROM [CurriculumTerms] ct
          WHERE ct.[ProjectId] = pm.[NewId] AND ct.[Name] = fd.[Name]);

    INSERT INTO @map (FolderId, EntityType, NewId)
    SELECT fd.[Id], N'term', MIN(ct.[Id])
    FROM #FolderDepth fd
    INNER JOIN @map pm ON pm.[FolderId] = fd.[ParentFolderId] AND pm.[EntityType] = N'project'
    INNER JOIN [CurriculumTerms] ct ON ct.[ProjectId] = pm.[NewId] AND ct.[Name] = fd.[Name]
    WHERE fd.[Depth] = 2
      AND NOT EXISTS (SELECT 1 FROM @map m WHERE m.FolderId = fd.[Id])
    GROUP BY fd.[Id];

    INSERT INTO [CurriculumTerms] ([ProjectId], [Name])
    SELECT lp.[ProjectId], fd.[Name]
    FROM #FolderDepth fd
    INNER JOIN @depth1 d1 ON d1.[Id] = fd.[Id] AND d1.[IsProject] = 0
    INNER JOIN @legacyProject lp ON lp.[RootId] = fd.[RootId]
    WHERE NOT EXISTS (
          SELECT 1 FROM [CurriculumTerms] ct
          WHERE ct.[ProjectId] = lp.[ProjectId] AND ct.[Name] = fd.[Name]);

    INSERT INTO @map (FolderId, EntityType, NewId)
    SELECT fd.[Id], N'term', MIN(ct.[Id])
    FROM #FolderDepth fd
    INNER JOIN @depth1 d1 ON d1.[Id] = fd.[Id] AND d1.[IsProject] = 0
    INNER JOIN @legacyProject lp ON lp.[RootId] = fd.[RootId]
    INNER JOIN [CurriculumTerms] ct ON ct.[ProjectId] = lp.[ProjectId] AND ct.[Name] = fd.[Name]
    WHERE NOT EXISTS (SELECT 1 FROM @map m WHERE m.FolderId = fd.[Id])
    GROUP BY fd.[Id];

    INSERT INTO [SubjectGroups] ([TermId], [Name])
    SELECT tm.[NewId], fd.[Name]
    FROM #FolderDepth fd
    INNER JOIN @map tm ON tm.[FolderId] = fd.[ParentFolderId] AND tm.[EntityType] = N'term'
    WHERE NOT EXISTS (
          SELECT 1 FROM [SubjectGroups] sg
          WHERE sg.[TermId] = tm.[NewId] AND sg.[Name] = fd.[Name]);

    INSERT INTO @map (FolderId, EntityType, NewId)
    SELECT fd.[Id], N'subjectGroup', MIN(sg.[Id])
    FROM #FolderDepth fd
    INNER JOIN @map tm ON tm.[FolderId] = fd.[ParentFolderId] AND tm.[EntityType] = N'term'
    INNER JOIN [SubjectGroups] sg ON sg.[TermId] = tm.[NewId] AND sg.[Name] = fd.[Name]
    WHERE NOT EXISTS (SELECT 1 FROM @map m WHERE m.FolderId = fd.[Id])
    GROUP BY fd.[Id];

    IF COL_LENGTH(N'dbo.Subjects', N'FolderId') IS NOT NULL
       AND COL_LENGTH(N'dbo.Subjects', N'SubjectGroupId') IS NOT NULL
    BEGIN
        UPDATE s
        SET s.[SubjectGroupId] = m.[NewId]
        FROM [Subjects] s
        INNER JOIN @map m ON m.[FolderId] = s.[FolderId] AND m.[EntityType] = N'subjectGroup';

        IF EXISTS (SELECT 1 FROM [Subjects] WHERE [SubjectGroupId] IS NULL)
        BEGIN
            DECLARE @FallbackGroupId INT = (SELECT TOP 1 [Id] FROM [SubjectGroups] ORDER BY [Id]);
            IF @FallbackGroupId IS NOT NULL
                UPDATE [Subjects] SET [SubjectGroupId] = @FallbackGroupId WHERE [SubjectGroupId] IS NULL;
        END
    END

    DROP TABLE #FolderDepth;
END
GO

-- ===== Verify =====
SELECT ay.Name AS [Year], p.Name AS Project, t.Name AS Term, sg.Name AS SubjectGroup, s.Name AS Subject
FROM AcademicYears ay
INNER JOIN CurriculumProjects p ON p.YearId = ay.Id
LEFT JOIN CurriculumTerms t ON t.ProjectId = p.Id
LEFT JOIN SubjectGroups sg ON sg.TermId = t.Id
LEFT JOIN Subjects s ON s.SubjectGroupId = sg.Id
ORDER BY p.Name, t.Name, sg.Name, s.Name;
GO
USE master;
GO
DROP DATABASE CurriculumMigTest;
