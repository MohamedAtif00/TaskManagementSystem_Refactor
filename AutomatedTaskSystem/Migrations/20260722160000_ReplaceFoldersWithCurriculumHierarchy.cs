using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceFoldersWithCurriculumHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.AcademicYears', N'U') IS NULL
                BEGIN
                    CREATE TABLE [AcademicYears] (
                        [Id] int NOT NULL IDENTITY,
                        [Name] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NULL,
                        CONSTRAINT [PK_AcademicYears] PRIMARY KEY ([Id])
                    );
                END

                IF OBJECT_ID(N'dbo.CurriculumProjects', N'U') IS NULL
                BEGIN
                    CREATE TABLE [CurriculumProjects] (
                        [Id] int NOT NULL IDENTITY,
                        [YearId] int NOT NULL,
                        [Name] nvarchar(max) NOT NULL,
                        [Description] nvarchar(max) NULL,
                        CONSTRAINT [PK_CurriculumProjects] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_CurriculumProjects_AcademicYears_YearId]
                            FOREIGN KEY ([YearId]) REFERENCES [AcademicYears]([Id]) ON DELETE NO ACTION
                    );
                END

                IF OBJECT_ID(N'dbo.CurriculumTerms', N'U') IS NULL
                BEGIN
                    CREATE TABLE [CurriculumTerms] (
                        [Id] int NOT NULL IDENTITY,
                        [ProjectId] int NOT NULL,
                        [Name] nvarchar(max) NOT NULL,
                        [StartDate] datetime2 NULL,
                        [EndDate] datetime2 NULL,
                        CONSTRAINT [PK_CurriculumTerms] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_CurriculumTerms_CurriculumProjects_ProjectId]
                            FOREIGN KEY ([ProjectId]) REFERENCES [CurriculumProjects]([Id]) ON DELETE NO ACTION
                    );
                END

                IF OBJECT_ID(N'dbo.SubjectGroups', N'U') IS NULL
                BEGIN
                    CREATE TABLE [SubjectGroups] (
                        [Id] int NOT NULL IDENTITY,
                        [TermId] int NOT NULL,
                        [Name] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_SubjectGroups] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_SubjectGroups_CurriculumTerms_TermId]
                            FOREIGN KEY ([TermId]) REFERENCES [CurriculumTerms]([Id]) ON DELETE NO ACTION
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumProjects_YearId' AND object_id = OBJECT_ID(N'dbo.CurriculumProjects'))
                    CREATE INDEX [IX_CurriculumProjects_YearId] ON [CurriculumProjects]([YearId]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CurriculumTerms_ProjectId' AND object_id = OBJECT_ID(N'dbo.CurriculumTerms'))
                    CREATE INDEX [IX_CurriculumTerms_ProjectId] ON [CurriculumTerms]([ProjectId]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SubjectGroups_TermId' AND object_id = OBJECT_ID(N'dbo.SubjectGroups'))
                    CREATE INDEX [IX_SubjectGroups_TermId] ON [SubjectGroups]([TermId]);
                """
            );

            // Separate batch: SQL Server resolves column names at batch compile time.
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.Subjects', N'SubjectGroupId') IS NULL
                BEGIN
                    IF COL_LENGTH(N'dbo.Subjects', N'FolderId') IS NOT NULL
                        ALTER TABLE [Subjects] ADD [SubjectGroupId] INT NULL;
                    ELSE
                        ALTER TABLE [Subjects] ADD [SubjectGroupId] INT NOT NULL CONSTRAINT [DF_Subjects_SubjectGroupId] DEFAULT 0;
                END
                """
            );

            migrationBuilder.Sql(
                """
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

                    -- Classify each depth-1 folder. Legacy data (Season > Term > Subject family)
                    -- had no Project level, so its "terms" sit at depth 1 with children but no
                    -- grandchildren. Real projects either have grandchildren or no children yet.
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

                    -- Project-like depth-1 folders become projects.
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

                    -- Legacy terms need a project. Reuse an existing project in the year
                    -- (e.g. Selah Eltelmeez) instead of creating a duplicate year-named row.
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

                    -- Terms under real projects (standard shape, depth 2).
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

                    -- Legacy terms (term-like depth-1 folders) under the default project.
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

                    -- Subject groups: any folder whose parent was mapped to a term.
                    -- Covers both shapes (depth 3 standard, depth 2 legacy).
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
                """
            );

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.Subjects', N'FolderId') IS NOT NULL
                   AND COL_LENGTH(N'dbo.Subjects', N'SubjectGroupId') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM [AcademicYears])
                BEGIN
                    INSERT INTO [AcademicYears] ([Name]) VALUES (N'2026/2027');
                    DECLARE @YearId INT = SCOPE_IDENTITY();
                    INSERT INTO [CurriculumProjects] ([YearId], [Name]) VALUES (@YearId, N'Selah Eltelmeez');
                    DECLARE @ProjectId INT = SCOPE_IDENTITY();
                    INSERT INTO [CurriculumTerms] ([ProjectId], [Name]) VALUES (@ProjectId, N'Term 1');
                    DECLARE @TermId INT = SCOPE_IDENTITY();
                    INSERT INTO [SubjectGroups] ([TermId], [Name]) VALUES (@TermId, N'Other');
                    DECLARE @GroupId INT = SCOPE_IDENTITY();
                    UPDATE [Subjects] SET [SubjectGroupId] = @GroupId WHERE [SubjectGroupId] IS NULL;
                END
                """
            );

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Subjects_Folders_FolderId')
                    ALTER TABLE [Subjects] DROP CONSTRAINT [FK_Subjects_Folders_FolderId];
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Subjects_FolderId' AND object_id = OBJECT_ID(N'dbo.Subjects'))
                    DROP INDEX [IX_Subjects_FolderId] ON [Subjects];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Folders_FolderProjects_ProjectId')
                    ALTER TABLE [Folders] DROP CONSTRAINT [FK_Folders_FolderProjects_ProjectId];
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Folders_Folders_ParentFolderId')
                    ALTER TABLE [Folders] DROP CONSTRAINT [FK_Folders_Folders_ParentFolderId];
                IF OBJECT_ID(N'dbo.Folders', N'U') IS NOT NULL
                    DROP TABLE [Folders];
                IF OBJECT_ID(N'dbo.FolderProjects', N'U') IS NOT NULL
                    DROP TABLE [FolderProjects];
                IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.Subjects', N'FolderId') IS NOT NULL
                    ALTER TABLE [Subjects] DROP COLUMN [FolderId];
                """
            );

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.Subjects', N'SubjectGroupId') IS NOT NULL
                   AND EXISTS (SELECT 1 FROM [Subjects] WHERE [SubjectGroupId] IS NULL)
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM [SubjectGroups])
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM [AcademicYears])
                            INSERT INTO [AcademicYears] ([Name]) VALUES (N'2026/2027');
                        DECLARE @RepairYearId INT = (SELECT TOP 1 [Id] FROM [AcademicYears] ORDER BY [Id]);
                        IF NOT EXISTS (SELECT 1 FROM [CurriculumProjects])
                            INSERT INTO [CurriculumProjects] ([YearId], [Name]) VALUES (@RepairYearId, N'Selah Eltelmeez');
                        DECLARE @RepairProjectId INT = (SELECT TOP 1 [Id] FROM [CurriculumProjects] ORDER BY [Id]);
                        IF NOT EXISTS (SELECT 1 FROM [CurriculumTerms])
                            INSERT INTO [CurriculumTerms] ([ProjectId], [Name]) VALUES (@RepairProjectId, N'Term 1');
                        DECLARE @RepairTermId INT = (SELECT TOP 1 [Id] FROM [CurriculumTerms] ORDER BY [Id]);
                        IF NOT EXISTS (SELECT 1 FROM [SubjectGroups])
                            INSERT INTO [SubjectGroups] ([TermId], [Name]) VALUES (@RepairTermId, N'Other');
                    END

                    DECLARE @RepairGroupId INT = (SELECT TOP 1 [Id] FROM [SubjectGroups] ORDER BY [Id]);
                    IF @RepairGroupId IS NOT NULL
                        UPDATE [Subjects] SET [SubjectGroupId] = @RepairGroupId WHERE [SubjectGroupId] IS NULL;
                END
                """
            );

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.Subjects', N'SubjectGroupId') IS NOT NULL
                   AND EXISTS (
                        SELECT 1
                        FROM sys.columns
                        WHERE object_id = OBJECT_ID(N'dbo.Subjects')
                          AND name = N'SubjectGroupId'
                          AND is_nullable = 1)
                    ALTER TABLE [Subjects] ALTER COLUMN [SubjectGroupId] INT NOT NULL;

                IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = N'DF_Subjects_SubjectGroupId')
                    ALTER TABLE [Subjects] DROP CONSTRAINT [DF_Subjects_SubjectGroupId];

                IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.Subjects', N'SubjectGroupId') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Subjects_SubjectGroupId' AND object_id = OBJECT_ID(N'dbo.Subjects'))
                    CREATE INDEX [IX_Subjects_SubjectGroupId] ON [Subjects]([SubjectGroupId]);

                IF OBJECT_ID(N'dbo.Subjects', N'U') IS NOT NULL
                   AND OBJECT_ID(N'dbo.SubjectGroups', N'U') IS NOT NULL
                   AND COL_LENGTH(N'dbo.Subjects', N'SubjectGroupId') IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Subjects_SubjectGroups_SubjectGroupId')
                    ALTER TABLE [Subjects] ADD CONSTRAINT [FK_Subjects_SubjectGroups_SubjectGroupId]
                        FOREIGN KEY ([SubjectGroupId]) REFERENCES [SubjectGroups]([Id]) ON DELETE NO ACTION;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_SubjectGroups_SubjectGroupId",
                table: "Subjects");

            migrationBuilder.DropTable(name: "SubjectGroups");
            migrationBuilder.DropTable(name: "CurriculumTerms");
            migrationBuilder.DropTable(name: "CurriculumProjects");
            migrationBuilder.DropTable(name: "AcademicYears");

            migrationBuilder.DropIndex(name: "IX_Subjects_SubjectGroupId", table: "Subjects");
            migrationBuilder.DropColumn(name: "SubjectGroupId", table: "Subjects");

            migrationBuilder.CreateTable(
                name: "FolderProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LevelNamesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentFolderId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    LevelName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_FolderProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "FolderProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "Subjects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentFolderId",
                table: "Folders",
                column: "ParentFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ProjectId",
                table: "Folders",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_FolderId",
                table: "Subjects",
                column: "FolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Folders_FolderId",
                table: "Subjects",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
