using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    /// <inheritdoc />
    public partial class FolderHierarchyReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Terms_TermId",
                table: "Subjects");

            migrationBuilder.DropTable(
                name: "Terms");

            migrationBuilder.DropTable(
                name: "ProjectYears");

            migrationBuilder.DropTable(
                name: "RootProjects");

            migrationBuilder.RenameColumn(
                name: "TermId",
                table: "Subjects",
                newName: "FolderId");

            migrationBuilder.RenameIndex(
                name: "IX_Subjects_TermId",
                table: "Subjects",
                newName: "IX_Subjects_FolderId");

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentFolderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentFolderId",
                table: "Folders",
                column: "ParentFolderId");

            // Reset strategy: build Selah Eltelmeez > 2026/2027 > Term 1 / Term 2
            // > subject folders (Arabic, English, ...), then place every subject into
            // its subject folder. Grade remains encoded in the subject name, so entering
            // a subject folder shows all grades for that subject.
            migrationBuilder.Sql(
                """
                DECLARE @RootId INT;
                DECLARE @YearId INT;
                DECLARE @Term1Id INT;
                DECLARE @Term2Id INT;

                INSERT INTO [Folders] ([Name], [ParentFolderId]) VALUES (N'Selah Eltelmeez', NULL);
                SET @RootId = SCOPE_IDENTITY();

                INSERT INTO [Folders] ([Name], [ParentFolderId]) VALUES (N'2026/2027', @RootId);
                SET @YearId = SCOPE_IDENTITY();

                INSERT INTO [Folders] ([Name], [ParentFolderId]) VALUES (N'Term 1', @YearId);
                SET @Term1Id = SCOPE_IDENTITY();

                INSERT INTO [Folders] ([Name], [ParentFolderId]) VALUES (N'Term 2', @YearId);
                SET @Term2Id = SCOPE_IDENTITY();

                DECLARE @Map TABLE
                (
                    SubjectId INT NOT NULL,
                    TermFolderId INT NOT NULL,
                    SubjectFolder NVARCHAR(100) NOT NULL
                );

                INSERT INTO @Map (SubjectId, TermFolderId, SubjectFolder)
                SELECT
                    s.[Id],
                    CASE
                        WHEN n.NameL LIKE N'%[_]2a%' OR n.NameL LIKE N'%[_]2e%' THEN @Term2Id
                        WHEN n.NameL LIKE N'%[_]1a%' OR n.NameL LIKE N'%[_]1e%' THEN @Term1Id
                        WHEN n.NameL LIKE N'%term%2%' THEN @Term2Id
                        ELSE @Term1Id
                    END,
                    CASE p.Prefix
                        WHEN 'ara' THEN N'Arabic'
                        WHEN 'eng' THEN N'English'
                        WHEN 'mth' THEN N'Math'
                        WHEN 'sci' THEN N'Science'
                        WHEN 'soc' THEN N'Social Studies'
                        WHEN 'ict' THEN N'ICT'
                        WHEN 'mul' THEN N'Multimedia'
                        WHEN 'rel' THEN N'Religion'
                        ELSE N'Other'
                    END
                FROM [Subjects] s
                CROSS APPLY (SELECT LOWER(LTRIM(RTRIM(s.[Name]))) AS NameL) n
                CROSS APPLY (SELECT CASE
                    WHEN CHARINDEX('_', n.NameL) > 1 THEN LEFT(n.NameL, CHARINDEX('_', n.NameL) - 1)
                    ELSE '' END AS Prefix) p;

                INSERT INTO [Folders] ([Name], [ParentFolderId])
                SELECT DISTINCT m.SubjectFolder, m.TermFolderId
                FROM @Map m;

                UPDATE s
                SET s.[FolderId] = sf.[Id]
                FROM [Subjects] s
                INNER JOIN @Map m ON m.SubjectId = s.[Id]
                INNER JOIN [Folders] sf
                    ON sf.[ParentFolderId] = m.TermFolderId AND sf.[Name] = m.SubjectFolder;
                """
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Folders_FolderId",
                table: "Subjects",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Folders_FolderId",
                table: "Subjects");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.RenameColumn(
                name: "FolderId",
                table: "Subjects",
                newName: "TermId");

            migrationBuilder.RenameIndex(
                name: "IX_Subjects_FolderId",
                table: "Subjects",
                newName: "IX_Subjects_TermId");

            migrationBuilder.CreateTable(
                name: "RootProjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RootProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RootProjectId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectYears", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectYears_RootProjects_RootProjectId",
                        column: x => x.RootProjectId,
                        principalTable: "RootProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Terms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectYearId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Terms_ProjectYears_ProjectYearId",
                        column: x => x.ProjectYearId,
                        principalTable: "ProjectYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectYears_RootProjectId",
                table: "ProjectYears",
                column: "RootProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Terms_ProjectYearId",
                table: "Terms",
                column: "ProjectYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Terms_TermId",
                table: "Subjects",
                column: "TermId",
                principalTable: "Terms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
