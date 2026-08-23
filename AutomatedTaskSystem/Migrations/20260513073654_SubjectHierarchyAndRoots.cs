using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
    /// <inheritdoc />
    public partial class SubjectHierarchyAndRoots : Migration
    {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RootProjects",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_RootProjects", x => x.Id); });

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [RootProjects] WHERE Id = 1)
            BEGIN
                SET IDENTITY_INSERT [RootProjects] ON;
                INSERT INTO [RootProjects] (Id, Name, Description) VALUES (1, N'Selah Eltelmeez', N'');
                SET IDENTITY_INSERT [RootProjects] OFF;
            END
            """
        );

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
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Order = table.Column<int>(type: "int", nullable: false)
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

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM ProjectYears WHERE RootProjectId = 1 AND Label = N'2026/2027')
                INSERT INTO ProjectYears (RootProjectId, Label) VALUES (1, N'2026/2027');

            IF NOT EXISTS (
                SELECT 1 FROM Terms t
                INNER JOIN ProjectYears py ON py.Id = t.ProjectYearId
                WHERE py.RootProjectId = 1 AND py.Label = N'2026/2027' AND t.Name = N'Term 1')
                INSERT INTO Terms (ProjectYearId, Name, [Order])
                SELECT py.Id, N'Term 1', 0 FROM ProjectYears py WHERE py.RootProjectId = 1 AND py.Label = N'2026/2027';

            IF NOT EXISTS (
                SELECT 1 FROM Terms t
                INNER JOIN ProjectYears py ON py.Id = t.ProjectYearId
                WHERE py.RootProjectId = 1 AND py.Label = N'2026/2027' AND t.Name = N'Term 2')
                INSERT INTO Terms (ProjectYearId, Name, [Order])
                SELECT py.Id, N'Term 2', 1 FROM ProjectYears py WHERE py.RootProjectId = 1 AND py.Label = N'2026/2027';

            IF NOT EXISTS (
                SELECT 1 FROM Terms t
                INNER JOIN ProjectYears py ON py.Id = t.ProjectYearId
                WHERE py.RootProjectId = 1 AND py.Label <> N'2026/2027' AND t.Name = N'Term 2')
                INSERT INTO Terms (ProjectYearId, Name, [Order])
                SELECT py.Id, N'Term 2', 1
                FROM ProjectYears py
                WHERE py.RootProjectId = 1 AND py.Label <> N'2026/2027';
            """
        );

        migrationBuilder.AddColumn<int>(
            name: "TermId",
            table: "Projects",
            type: "int",
            nullable: true);

        migrationBuilder.Sql(
            """
            UPDATE p
            SET TermId = t.Id
            FROM Projects p
            INNER JOIN Years y ON y.Id = p.YearId
            INNER JOIN ProjectYears py ON py.RootProjectId = 1 AND py.Label = y.Number
            INNER JOIN Terms t ON t.ProjectYearId = py.Id
                AND t.Name = CASE
                    WHEN py.Label = N'2026/2027' THEN N'Term 1'
                    WHEN p.Term = 1 THEN N'Term2'
                    ELSE N'Term1'
                END;
            """
        );

        migrationBuilder.Sql(
            """
            UPDATE Projects
            SET TermId = (SELECT TOP 1 Id FROM Terms ORDER BY Id)
            WHERE TermId IS NULL;
            """
        );

        migrationBuilder.AlterColumn<int>(
            name: "TermId",
            table: "Projects",
            type: "int",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.DropForeignKey(
            name: "FK_Projects_Years_YearId",
            table: "Projects");

        migrationBuilder.DropIndex(
            name: "IX_Projects_YearId",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "YearId",
            table: "Projects");

        migrationBuilder.DropColumn(
            name: "Term",
            table: "Projects");

        migrationBuilder.DropForeignKey(
            name: "FK_Units_Projects_ProjectId",
            table: "Units");

        migrationBuilder.RenameTable(
            name: "Projects",
            newName: "Subjects");

        migrationBuilder.RenameColumn(
            name: "ProjectId",
            table: "Units",
            newName: "SubjectId");

        migrationBuilder.RenameIndex(
            name: "IX_Units_ProjectId",
            table: "Units",
            newName: "IX_Units_SubjectId");

        migrationBuilder.AddForeignKey(
            name: "FK_Units_Subjects_SubjectId",
            table: "Units",
            column: "SubjectId",
            principalTable: "Subjects",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Subjects_Terms_TermId",
            table: "Subjects",
            column: "TermId",
            principalTable: "Terms",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.CreateIndex(
            name: "IX_Subjects_TermId",
            table: "Subjects",
            column: "TermId");

        migrationBuilder.Sql(
            """
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ProjectUser_Projects_ProjectsId')
                ALTER TABLE ProjectUser DROP CONSTRAINT FK_ProjectUser_Projects_ProjectsId;
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ProjectUser_Users_UsersId')
                ALTER TABLE ProjectUser DROP CONSTRAINT FK_ProjectUser_Users_UsersId;
            """
        );

        migrationBuilder.RenameColumn(
            name: "ProjectsId",
            table: "ProjectUser",
            newName: "SubjectsId");

        migrationBuilder.RenameTable(
            name: "ProjectUser",
            newName: "SubjectUser");

        migrationBuilder.RenameIndex(
            name: "IX_ProjectUser_UsersId",
            table: "SubjectUser",
            newName: "IX_SubjectUser_UsersId");

        migrationBuilder.AddForeignKey(
            name: "FK_SubjectUser_Subjects_SubjectsId",
            table: "SubjectUser",
            column: "SubjectsId",
            principalTable: "Subjects",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_SubjectUser_Users_UsersId",
            table: "SubjectUser",
            column: "UsersId",
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.NoAction);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_SubjectUser_Subjects_SubjectsId",
            table: "SubjectUser");

        migrationBuilder.DropForeignKey(
            name: "FK_SubjectUser_Users_UsersId",
            table: "SubjectUser");

        migrationBuilder.DropForeignKey(
            name: "FK_Units_Subjects_SubjectId",
            table: "Units");

        migrationBuilder.DropForeignKey(
            name: "FK_Subjects_Terms_TermId",
            table: "Subjects");

        migrationBuilder.DropIndex(
            name: "IX_Subjects_TermId",
            table: "Subjects");

        migrationBuilder.DropTable(
            name: "SubjectUser");

        migrationBuilder.RenameTable(
            name: "Subjects",
            newName: "Projects");

        migrationBuilder.RenameColumn(
            name: "SubjectId",
            table: "Units",
            newName: "ProjectId");

        migrationBuilder.RenameIndex(
            name: "IX_Units_SubjectId",
            table: "Units",
            newName: "IX_Units_ProjectId");

        migrationBuilder.AddColumn<int>(
            name: "YearId",
            table: "Projects",
            type: "int",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddColumn<bool>(
            name: "Term",
            table: "Projects",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.Sql(
            """
            UPDATE p
            SET YearId = y.Id, Term = CASE WHEN t.Name = N'Term2' THEN 1 ELSE 0 END
            FROM Projects p
            INNER JOIN Terms t ON t.Id = p.TermId
            INNER JOIN ProjectYears py ON py.Id = t.ProjectYearId
            INNER JOIN Years y ON y.Number = py.Label;
            """
        );

        migrationBuilder.DropColumn(
            name: "TermId",
            table: "Projects");

        migrationBuilder.AddForeignKey(
            name: "FK_Projects_Years_YearId",
            table: "Projects",
            column: "YearId",
            principalTable: "Years",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.CreateIndex(
            name: "IX_Projects_YearId",
            table: "Projects",
            column: "YearId");

        migrationBuilder.AddForeignKey(
            name: "FK_Units_Projects_ProjectId",
            table: "Units",
            column: "ProjectId",
            principalTable: "Projects",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.DropTable(
            name: "Terms");

        migrationBuilder.DropTable(
            name: "ProjectYears");

        migrationBuilder.Sql("DELETE FROM [RootProjects] WHERE Id = 1;");

        migrationBuilder.DropTable(
            name: "RootProjects");
    }
    }
}
