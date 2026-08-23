using Microsoft.EntityFrameworkCore.Migrations;



#nullable disable



namespace AutomatedTaskSystem.Migrations

{

    /// <inheritdoc />

    public partial class ApplyCurriculumHierarchyToFolders : Migration

    {

        /// <inheritdoc />

        protected override void Up(MigrationBuilder migrationBuilder)

        {

            migrationBuilder.AddColumn<string>(

                name: "LevelName",

                table: "Folders",

                type: "nvarchar(32)",

                maxLength: 32,

                nullable: false,

                defaultValue: "");



            migrationBuilder.Sql(

                """

                UPDATE [FolderProjects]

                SET [LevelNamesJson] = N'["Project","Term","Subject"]'

                WHERE [LevelNamesJson] IS NULL

                   OR [LevelNamesJson] = N'["Years","Terms","Subjects"]'

                   OR [LevelNamesJson] = N'["Season","Term","Family"]'

                   OR [LevelNamesJson] = N'["Program","Term","Family"]';

                """

            );



            // Old layout: Project name at root, Season (e.g. 2026/2027) as child.

            // Target: Season at root, Project as child.

            migrationBuilder.Sql(

                """

                IF OBJECT_ID('tempdb..#HierarchySwap') IS NOT NULL DROP TABLE #HierarchySwap;



                SELECT

                    r.[Id] AS ProjectRootId,

                    s.[Id] AS SeasonId,

                    r.[ProjectId],

                    s.[Name] AS SeasonName

                INTO #HierarchySwap

                FROM [Folders] r

                INNER JOIN [Folders] s ON s.[ParentFolderId] = r.[Id]

                WHERE r.[ParentFolderId] IS NULL

                  AND s.[Name] LIKE N'%/%'

                  AND r.[Name] NOT LIKE N'%/%';



                UPDATE s

                SET s.[ParentFolderId] = NULL

                FROM [Folders] s

                INNER JOIN #HierarchySwap hs ON hs.[SeasonId] = s.[Id];



                UPDATE r

                SET r.[ParentFolderId] = hs.[SeasonId]

                FROM [Folders] r

                INNER JOIN #HierarchySwap hs ON hs.[ProjectRootId] = r.[Id];



                UPDATE fp

                SET fp.[Name] = hs.[SeasonName]

                FROM [FolderProjects] fp

                INNER JOIN #HierarchySwap hs ON fp.[Id] = hs.[ProjectId];



                DROP TABLE #HierarchySwap;

                """

            );



            migrationBuilder.Sql(

                """

                ;WITH [FolderDepth] AS (

                    SELECT [Id], [ParentFolderId], 0 AS [Depth]

                    FROM [Folders]

                    WHERE [ParentFolderId] IS NULL

                    UNION ALL

                    SELECT f.[Id], f.[ParentFolderId], fd.[Depth] + 1

                    FROM [Folders] f

                    INNER JOIN [FolderDepth] fd ON f.[ParentFolderId] = fd.[Id]

                )

                UPDATE f

                SET f.[LevelName] = CASE fd.[Depth]

                    WHEN 0 THEN N'Season'

                    WHEN 1 THEN N'Project'

                    WHEN 2 THEN N'Term'

                    ELSE N'Subject'

                END

                FROM [Folders] f

                INNER JOIN [FolderDepth] fd ON f.[Id] = fd.[Id];

                """

            );

        }



        /// <inheritdoc />

        protected override void Down(MigrationBuilder migrationBuilder)

        {

            migrationBuilder.DropColumn(

                name: "LevelName",

                table: "Folders");

        }

    }

}


