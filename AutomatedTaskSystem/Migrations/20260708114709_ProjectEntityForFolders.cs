using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    /// <inheritdoc />
    public partial class ProjectEntityForFolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FolderProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FolderProjects", x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Folders",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                DECLARE @RootProjectMap TABLE
                (
                    RootId INT NOT NULL,
                    ProjectId INT NOT NULL
                );

                MERGE INTO [FolderProjects] AS tgt
                USING (
                    SELECT r.[Id], r.[Name]
                    FROM [Folders] r
                    WHERE r.[ParentFolderId] IS NULL
                ) AS src
                ON 1 = 0
                WHEN NOT MATCHED THEN
                    INSERT ([Name], [Description]) VALUES (src.[Name], N'Auto-migrated project')
                OUTPUT src.[Id], inserted.[Id] INTO @RootProjectMap([RootId], [ProjectId]);

                ;WITH FolderTree AS
                (
                    SELECT r.[Id], r.[ParentFolderId], r.[Name], r.[Id] AS [RootId]
                    FROM [Folders] r
                    WHERE r.[ParentFolderId] IS NULL
                    UNION ALL
                    SELECT c.[Id], c.[ParentFolderId], c.[Name], ft.[RootId]
                    FROM [Folders] c
                    INNER JOIN FolderTree ft ON c.[ParentFolderId] = ft.[Id]
                )
                UPDATE f
                SET f.[ProjectId] = p.[ProjectId]
                FROM [Folders] f
                INNER JOIN FolderTree ft ON ft.[Id] = f.[Id]
                INNER JOIN @RootProjectMap p ON p.[RootId] = ft.[RootId];

                IF EXISTS (SELECT 1 FROM [Folders] WHERE [ProjectId] IS NULL)
                BEGIN
                    DECLARE @FallbackProjectId INT;
                    INSERT INTO [FolderProjects] ([Name], [Description]) VALUES (N'Default Project', N'Auto-created fallback project');
                    SET @FallbackProjectId = SCOPE_IDENTITY();
                    UPDATE [Folders] SET [ProjectId] = @FallbackProjectId WHERE [ProjectId] IS NULL;
                END
                """
            );

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "Folders",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ProjectId",
                table: "Folders",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_FolderProjects_ProjectId",
                table: "Folders",
                column: "ProjectId",
                principalTable: "FolderProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_FolderProjects_ProjectId",
                table: "Folders");

            migrationBuilder.DropTable(
                name: "FolderProjects");

            migrationBuilder.DropIndex(
                name: "IX_Folders_ProjectId",
                table: "Folders");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Folders");
        }
    }
}
