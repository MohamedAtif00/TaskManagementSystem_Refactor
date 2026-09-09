# Database (DbUp + SQL Server)

Script-based database management following [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd). Architecture overview: [ARCHITECTURE.md](ARCHITECTURE.md).

**Last updated:** 2026-09-05

## Approach

- **One database**, **one SQL schema per module** (plus `app` for infrastructure)
- **DbUp** runs ordered migration scripts; journal table: `app.MigrationsJournal`
- **No EF Core migrations** in this pass — SQL scripts are the source of deployment
- **Groups removed** from org model; `TeamId` replaces `GroupId` on `TaskBank` and `Tasks`

## Layout

```
src/Database/
  DatabaseMigrator/                         Console app (DbUp runner)
  TaskManagementSystem.Database/            SQL scripts project (visible in VS)
    TaskManagementSystem.Database.csproj
    Scripts/
      CreateDatabase.sql
      InitializeDatabase.sql
      Migrations/                           DbUp executes these (alphabetical)
      Seeds/                                Optional manual seeds
    Structure/                              Per-table DDL source files
      {schema}/Tables/*.sql
```

## Schemas and tables

| Schema | Module | Tables |
|---|---|---|
| `app` | Infrastructure | `MigrationsJournal` (DbUp) |
| `identity` | Identity | `Users`, `RefreshTokens`, `UserChanges` |
| `organization` | Organization | `Teams`, `Sections`, `SectionTeams` |
| `workflows` | Workflows | `SchemaTypes`, `Schemas`, `Nodes`, `Steps`, `TaskBank`, `NodeSequences`, `RSteps` |
| `curriculum` | Curriculum | `AcademicYears`, `CurriculumProjects`, `CurriculumTerms`, `SubjectGroups`, `Subjects`, `Units`, `Lessons`, `LearningObjectives`, `SubjectUser` |
| `ticket` | Ticket | `Tasks`, `TaskActivities`, `TaskWorkTimes`, `Comments`, `Rollbacks`, `RollbackIssues` |
| `sprints` | Sprints | `Sprints`, `SprintLearningObjectives` |
| `notifications` | Notifications | `Notifications` |
| `hr` | HR | `LeaveRequests`, `LeaveResetLogs`, `Permissions`, `WorkFromHomeRequests`, `Opinions` |

**Not ported:** legacy `Groups`, `SectionGroups`, `Years` (use `curriculum.AcademicYears`).

## Local setup

### 1. Create the database (optional)

Run `Scripts/CreateDatabase.sql` against `master` if the database does not exist yet.

### 2. Run migrations

From the solution root:

```powershell
dotnet run --project src/Database/DatabaseMigrator `
  "Server=localhost;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True" `
  "src/Database/TaskManagementSystem.Database/Scripts/Migrations"
```

For LocalDB:

```powershell
dotnet run --project src/Database/DatabaseMigrator `
  "Server=(localdb)\MSSQLLocalDB;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True" `
  "src/Database/TaskManagementSystem.Database/Scripts/Migrations"
```

### 3. Verify

```sql
SELECT name FROM sys.schemas ORDER BY name;
SELECT COUNT(*) FROM app.MigrationsJournal;
-- Expect 12 migration entries (000-011)
```

## Connection string (API)

Placeholder in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True"
}
```

The API does **not** run DbUp on startup yet — run `DatabaseMigrator` explicitly (or from CI).

## Adding a schema change

1. Edit the table under `Structure/{schema}/Tables/`.
2. Add a new script `Scripts/Migrations/011_description.sql`.
3. Run `DatabaseMigrator` again (DbUp skips scripts already in the journal).

## Migration order

| Script | Content |
|---|---|
| `000_app_CreateAppSchema.sql` | Create `app` schema (also ensured by migrator before DbUp journal init) |
| `001_app_CreateSchemas.sql` | Create all module schemas |
| `002_organization_Tables.sql` | Teams, Sections, SectionTeams |
| `003_identity_Tables.sql` | Users, RefreshTokens, UserChanges + Sections.HeadId FK |
| `004_workflows_Tables.sql` | Workflow engine tables |
| `005_curriculum_Tables.sql` | Curriculum hierarchy |
| `006_ticket_Tables.sql` | Ticket execution tables |
| `007_sprints_Tables.sql` | Sprint planning |
| `008_notifications_Tables.sql` | Notifications inbox |
| `009_hr_Tables.sql` | Leave, permissions, WFH |
| `010_seeds_ReferenceData.sql` | Reference seed data |
| `011_identity_SeedUsers.sql` | Integration/dev test user `TST001` + Integration Test Team |
