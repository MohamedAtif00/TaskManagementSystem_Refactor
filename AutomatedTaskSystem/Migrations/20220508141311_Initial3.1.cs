using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutomatedTaskSystem.Migrations
{
	public partial class Initial31 : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "AspNetRoles",
				columns: table => new
				{
					Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
					Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetRoles", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Groups",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Groups", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "LeaningObjectivesTypes",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_LeaningObjectivesTypes", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Projects",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Projects", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "TaskStatus",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_TaskStatus", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Titles",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Titles", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "AspNetRoleClaims",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
					table.ForeignKey(
						name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
						column: x => x.RoleId,
						principalTable: "AspNetRoles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Teams",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
					GroupId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Teams", x => x.Id);
					table.ForeignKey(
						name: "FK_Teams_Groups_GroupId",
						column: x => x.GroupId,
						principalTable: "Groups",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Schemas",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
					IsStart = table.Column<bool>(type: "bit", nullable: false),
					Serial = table.Column<int>(type: "int", nullable: false),
					Next = table.Column<int>(type: "int", nullable: false),
					Duration = table.Column<int>(type: "int", nullable: false),
					GroupId = table.Column<int>(type: "int", nullable: false),
					TypeId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Schemas", x => x.Id);
					table.ForeignKey(
						name: "FK_Schemas_Groups_GroupId",
						column: x => x.GroupId,
						principalTable: "Groups",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_Schemas_LeaningObjectivesTypes_TypeId",
						column: x => x.TypeId,
						principalTable: "LeaningObjectivesTypes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "LearningObjectives",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
					Unit = table.Column<int>(type: "int", nullable: false),
					Lesson = table.Column<int>(type: "int", nullable: false),
					TypeId = table.Column<int>(type: "int", nullable: false),
					ProjectId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_LearningObjectives", x => x.Id);
					table.ForeignKey(
						name: "FK_LearningObjectives_LeaningObjectivesTypes_TypeId",
						column: x => x.TypeId,
						principalTable: "LeaningObjectivesTypes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_LearningObjectives_Projects_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Projects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUsers",
				columns: table => new
				{
					Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
					TitleId = table.Column<int>(type: "int", nullable: false),
					UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
					EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
					PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
					SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
					PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
					PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
					TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
					LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
					LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
					AccessFailedCount = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUsers", x => x.Id);
					table.ForeignKey(
						name: "FK_AspNetUsers_Titles_TitleId",
						column: x => x.TitleId,
						principalTable: "Titles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AppUserProject",
				columns: table => new
				{
					ProjectsId = table.Column<int>(type: "int", nullable: false),
					UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AppUserProject", x => new { x.ProjectsId, x.UsersId });
					table.ForeignKey(
						name: "FK_AppUserProject_AspNetUsers_UsersId",
						column: x => x.UsersId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_AppUserProject_Projects_ProjectsId",
						column: x => x.ProjectsId,
						principalTable: "Projects",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AppUserTeam",
				columns: table => new
				{
					TeamsId = table.Column<int>(type: "int", nullable: false),
					UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AppUserTeam", x => new { x.TeamsId, x.UsersId });
					table.ForeignKey(
						name: "FK_AppUserTeam_AspNetUsers_UsersId",
						column: x => x.UsersId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_AppUserTeam_Teams_TeamsId",
						column: x => x.TeamsId,
						principalTable: "Teams",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserClaims",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
					table.ForeignKey(
						name: "FK_AspNetUserClaims_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserLogins",
				columns: table => new
				{
					LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
					ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
					ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
					table.ForeignKey(
						name: "FK_AspNetUserLogins_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserRoles",
				columns: table => new
				{
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
					table.ForeignKey(
						name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
						column: x => x.RoleId,
						principalTable: "AspNetRoles",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_AspNetUserRoles_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "AspNetUserTokens",
				columns: table => new
				{
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
					LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
					Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
					Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
					table.ForeignKey(
						name: "FK_AspNetUserTokens_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Tasks",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					IsRollback = table.Column<bool>(type: "bit", nullable: false),
					due = table.Column<DateTime>(type: "datetime2", nullable: false),
					UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
					GroupId = table.Column<int>(type: "int", nullable: false),
					ProjectId = table.Column<int>(type: "int", nullable: false),
					LearningObjectiveId = table.Column<int>(type: "int", nullable: false),
					ParentId = table.Column<int>(type: "int", nullable: true),
					SchemaId = table.Column<int>(type: "int", nullable: false),
					StatusId = table.Column<int>(type: "int", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Tasks", x => x.Id);
					table.ForeignKey(
						name: "FK_Tasks_AspNetUsers_UserId",
						column: x => x.UserId,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onUpdate: ReferentialAction.NoAction);
					table.ForeignKey(
						name: "FK_Tasks_Groups_GroupId",
						column: x => x.GroupId,
						principalTable: "Groups",
						principalColumn: "Id",
						onDelete: ReferentialAction.NoAction,
						onUpdate: ReferentialAction.NoAction);
					table.ForeignKey(
						name: "FK_Tasks_LearningObjectives_LearningObjectiveId",
						column: x => x.LearningObjectiveId,
						principalTable: "LearningObjectives",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade,
						onUpdate: ReferentialAction.NoAction);
					table.ForeignKey(
						name: "FK_Tasks_Projects_ProjectId",
						column: x => x.ProjectId,
						principalTable: "Projects",
						principalColumn: "Id",
						onDelete: ReferentialAction.NoAction,
						onUpdate: ReferentialAction.NoAction);
					table.ForeignKey(
						name: "FK_Tasks_Schemas_SchemaId",
						column: x => x.SchemaId,
						principalTable: "Schemas",
						principalColumn: "Id",
						onDelete: ReferentialAction.NoAction,
						onUpdate: ReferentialAction.NoAction);
					table.ForeignKey(
						name: "FK_Tasks_Tasks_ParentId",
						column: x => x.ParentId,
						principalTable: "Tasks",
						principalColumn: "Id");
					table.ForeignKey(
						name: "FK_Tasks_TaskStatus_StatusId",
						column: x => x.StatusId,
						principalTable: "TaskStatus",
						principalColumn: "Id",
						onDelete: ReferentialAction.NoAction,
						onUpdate: ReferentialAction.NoAction);
				});

			migrationBuilder.CreateIndex(
				name: "IX_AppUserProject_UsersId",
				table: "AppUserProject",
				column: "UsersId");

			migrationBuilder.CreateIndex(
				name: "IX_AppUserTeam_UsersId",
				table: "AppUserTeam",
				column: "UsersId");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetRoleClaims_RoleId",
				table: "AspNetRoleClaims",
				column: "RoleId");

			migrationBuilder.CreateIndex(
				name: "RoleNameIndex",
				table: "AspNetRoles",
				column: "NormalizedName",
				unique: true,
				filter: "[NormalizedName] IS NOT NULL");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserClaims_UserId",
				table: "AspNetUserClaims",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserLogins_UserId",
				table: "AspNetUserLogins",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUserRoles_RoleId",
				table: "AspNetUserRoles",
				column: "RoleId");

			migrationBuilder.CreateIndex(
				name: "EmailIndex",
				table: "AspNetUsers",
				column: "NormalizedEmail");

			migrationBuilder.CreateIndex(
				name: "IX_AspNetUsers_TitleId",
				table: "AspNetUsers",
				column: "TitleId");

			migrationBuilder.CreateIndex(
				name: "UserNameIndex",
				table: "AspNetUsers",
				column: "NormalizedUserName",
				unique: true,
				filter: "[NormalizedUserName] IS NOT NULL");

			migrationBuilder.CreateIndex(
				name: "IX_LearningObjectives_ProjectId",
				table: "LearningObjectives",
				column: "ProjectId");

			migrationBuilder.CreateIndex(
				name: "IX_LearningObjectives_TypeId",
				table: "LearningObjectives",
				column: "TypeId");

			migrationBuilder.CreateIndex(
				name: "IX_Schemas_GroupId",
				table: "Schemas",
				column: "GroupId");

			migrationBuilder.CreateIndex(
				name: "IX_Schemas_TypeId",
				table: "Schemas",
				column: "TypeId");

			migrationBuilder.CreateIndex(
				name: "IX_Tasks_GroupId",
				table: "Tasks",
				column: "GroupId");

			migrationBuilder.CreateIndex(
				name: "IX_Tasks_LearningObjectiveId",
				table: "Tasks",
				column: "LearningObjectiveId");

			migrationBuilder.CreateIndex(
				name: "IX_Tasks_ParentId",
				table: "Tasks",
				column: "ParentId");

			migrationBuilder.CreateIndex(
				name: "IX_Tasks_ProjectId",
				table: "Tasks",
				column: "ProjectId");

			migrationBuilder.CreateIndex(
				name: "IX_Tasks_SchemaId",
				table: "Tasks",
				column: "SchemaId");

			migrationBuilder.CreateIndex(
				name: "IX_Tasks_StatusId",
				table: "Tasks",
				column: "StatusId");

			migrationBuilder.CreateIndex(
				name: "IX_Tasks_UserId",
				table: "Tasks",
				column: "UserId");

			migrationBuilder.CreateIndex(
				name: "IX_Teams_GroupId",
				table: "Teams",
				column: "GroupId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "AppUserProject");

			migrationBuilder.DropTable(
				name: "AppUserTeam");

			migrationBuilder.DropTable(
				name: "AspNetRoleClaims");

			migrationBuilder.DropTable(
				name: "AspNetUserClaims");

			migrationBuilder.DropTable(
				name: "AspNetUserLogins");

			migrationBuilder.DropTable(
				name: "AspNetUserRoles");

			migrationBuilder.DropTable(
				name: "AspNetUserTokens");

			migrationBuilder.DropTable(
				name: "Tasks");

			migrationBuilder.DropTable(
				name: "Teams");

			migrationBuilder.DropTable(
				name: "AspNetRoles");

			migrationBuilder.DropTable(
				name: "AspNetUsers");

			migrationBuilder.DropTable(
				name: "LearningObjectives");

			migrationBuilder.DropTable(
				name: "Schemas");

			migrationBuilder.DropTable(
				name: "TaskStatus");

			migrationBuilder.DropTable(
				name: "Titles");

			migrationBuilder.DropTable(
				name: "Projects");

			migrationBuilder.DropTable(
				name: "Groups");

			migrationBuilder.DropTable(
				name: "LeaningObjectivesTypes");
		}
	}
}
