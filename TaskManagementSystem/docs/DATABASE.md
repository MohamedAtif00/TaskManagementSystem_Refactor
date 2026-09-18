# Database (DbUp + SQL Server)

Script-based database management following [kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd). Architecture overview: [ARCHITECTURE.md](ARCHITECTURE.md).

**Last updated:** 2026-09-18 (seed separation, 19 migrations, RBAC permission catalog, outbox/inbox tables)

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
      Seeds/                                Test/dev data (not journaled; re-runnable)
    Structure/                              Per-table DDL source files
      {schema}/Tables/*.sql
```

## Schemas and tables

| Schema | Module | Tables |
|---|---|---|
| `app` | Infrastructure | `MigrationsJournal` (DbUp), `AuditLog` |
| `identity` | Identity | `Users`, `RefreshTokens`, `UserChanges`, `Permissions`, `Roles`, `RolePermissions`, `OutboxMessages`, `InboxMessages` |
| `organization` | Organization | `Teams`, `Sections`, `SectionTeams`, `OutboxMessages`, `InboxMessages` |
| `workflows` | Workflows | `SchemaTypes`, `Schemas`, `Nodes`, `Steps`, `TaskBank`, `NodeSequences`, `RSteps`, `OutboxMessages`, `InboxMessages` |
| `curriculum` | Curriculum | `AcademicYears`, `CurriculumProjects`, `CurriculumTerms`, `SubjectGroups`, `Subjects`, `Units`, `Lessons`, `LearningObjectives`, `SubjectUser`, `OutboxMessages`, `InboxMessages` |
| `ticket` | Ticket | `Tasks`, `TaskActivities`, `TaskWorkTimes`, `Comments`, `Rollbacks`, `RollbackIssues`, `OutboxMessages`, `InboxMessages` |
| `sprints` | Sprints | `Sprints`, `SprintLearningObjectives`, `OutboxMessages`, `InboxMessages` |
| `notifications` | Notifications | `Notifications`, `OutboxMessages`, `InboxMessages` |
| `hr` | HR | `LeaveRequests`, `LeaveResetLogs`, `Permissions`, `WorkFromHomeRequests`, `ForgotClockRequests`, `Opinions`, `EmployeeBalances`, `PublicHolidays`, `OutboxMessages`, `InboxMessages` |

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

If LocalDB fails to start (`sqllocaldb start MSSQLLocalDB`), repair SQL Server LocalDB or use a full SQL Server instance with the `localhost` connection string above. Integration tests and `DatabaseMigrator` both require a reachable SQL Server.

### 3. Verify

```sql
SELECT name FROM sys.schemas ORDER BY name;
SELECT COUNT(*) FROM app.MigrationsJournal;
-- Expect 19 migration entries (000-010, 012-019; 011 intentionally absent)
```

### 4. Seed integration / local dev test user (optional)

Migrations include reference data only (`010_seeds_ReferenceData.sql`, initial RBAC in `014`/`017`, full permission catalog in `019_identity_PermissionCatalog.sql`, employee-balance backfill in `015`). The integration test user (`TST001`, Integration Test Team) lives in seeds, not migrations:

```powershell
./scripts/seed-database.ps1

# LocalDB
./scripts/seed-database.ps1 `
  -ConnectionString "Server=(localdb)\MSSQLLocalDB;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True"
```

Seeds are **not** journaled — safe to re-run. Under the hood: `DatabaseMigrator --seed <connectionString> <pathToSeedScript>`.

Integration tests bootstrap all migrations, then run `Scripts/Seeds/002_IntegrationTestData.sql` automatically (including a guarded `hr.EmployeeBalances` row for `TST001`).

**Why `015` backfill is not enough alone:** migration `015_hr_EmployeeBalances.sql` only backfills when `[hr].[EmployeeBalances]` is empty. Production databases that already ran the old `011_identity_SeedUsers.sql` migration are unaffected. Fresh databases after seed separation have no users at migration time, so the seed script must insert the `TST001` balance row explicitly.

### Seed scripts

| Script | Purpose |
|---|---|
| `Scripts/Seeds/002_IntegrationTestData.sql` | `TST001`, Integration Test Team, guarded `hr.EmployeeBalances` (single source of truth) |
| `Scripts/Seeds/heavy-load/` | Optional performance/dev dataset (see below) |

Removed (do not restore): `Scripts/Migrations/011_identity_SeedUsers.sql`, `Scripts/Seeds/001_ReferenceData.sql` (duplicate of migration `010`).

## Connection string (API)

Placeholder in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True"
}
```

The API does **not** run DbUp on startup yet — run `DatabaseMigrator` explicitly (or from CI).

**Apidog:** import the full API spec from [`src/Api/TaskManagementSystem.Api/openapi/tms-openapi.json`](../src/Api/TaskManagementSystem.Api/openapi/tms-openapi.json) (also served at `GET /openapi/v1.json` when the API is running). Pair with [`openapi/apidog/environment.local-dev.json`](../src/Api/TaskManagementSystem.Api/openapi/apidog/environment.local-dev.json) for auto-token setup.

## Adding a schema change

1. Edit the table under `Structure/{schema}/Tables/`.
2. Add a new script `Scripts/Migrations/019_description.sql` (use the next free number; do not renumber existing scripts — DbUp journals by filename).
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
| `010_seeds_ReferenceData.sql` | Reference seed data (SchemaType, etc.) |
| *(no `011`)* | *Gap left after moving test/dev seed data out of migrations (DbUp journals by filename — do not renumber `012`–`018`)* |
| `012_app_AuditLog.sql` | Audit log table |
| `013_hr_PublicHolidays.sql` | Public holidays |
| `014_identity_Rbac.sql` | RBAC permissions and roles |
| `015_hr_EmployeeBalances.sql` | Employee leave balances |
| `016_hr_ForgotClockRequests.sql` | Forgot clock requests |
| `017_identity_UserAdminPermissions.sql` | User admin permissions |
| `018_outbox_inbox_Tables.sql` | `OutboxMessages` + `InboxMessages` in all 8 module schemas |
| `019_identity_PermissionCatalog.sql` | Full RBAC permission catalog (`{module}.{read\|create\|update\|delete\|manage}`), legacy code migration, default role grants (Owner gets all `*.manage`) |

**Module implementation status:** Curriculum, Ticket, Sprints, and Notifications application modules are implemented against the SQL schemas above. Sprints uses `007_sprints_Tables.sql`; Notifications uses `008_notifications_Tables.sql`.

## Migrations vs seeds

| Location | Purpose | Journaled? |
|---|---|---|
| `Scripts/Migrations/` | DDL + production reference data (SchemaType, RBAC roles/permissions, employee-balance backfill) | Yes (`app.MigrationsJournal`) |
| `Scripts/Seeds/` | Test/dev data (`TST001`, Integration Test Team, guarded `hr.EmployeeBalances` for that user) | No — re-runnable via `seed-database.ps1` or `DatabaseMigrator --seed` |

**DatabaseMigrator usage:**

```powershell
# Migrations (journaled)
dotnet run --project src/Database/DatabaseMigrator -- `
  "Server=localhost;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True" `
  "src/Database/TaskManagementSystem.Database/Scripts/Migrations"

# Seeds (not journaled; single file or folder of *.sql)
dotnet run --project src/Database/DatabaseMigrator -- --seed `
  "Server=localhost;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True" `
  "src/Database/TaskManagementSystem.Database/Scripts/Seeds/002_IntegrationTestData.sql"
```

## Optional dev heavy-load seed

For GET timing tests, Apidog scenarios, and local performance checks, you can populate medium-scale relational data (~50 users, ~432 LOs, ~2k tickets, ~5k notifications) without Mockaroo or DbUp.

Scripts live under `Scripts/Seeds/heavy-load/` (not in the migration journal). All synthetic rows use `SEED_*` name prefixes (users: codes `SD0001`–`SD0049`). Run `./scripts/seed-database.ps1` first if you need `TST001` / Integration Test Team alongside heavy-load data.

```powershell
# From TaskManagementSystem/ (solution root)
./scripts/seed-heavy-load.ps1

# LocalDB (default connection string)
./scripts/seed-heavy-load.ps1 `
  -ConnectionString "Server=(localdb)\MSSQLLocalDB;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True"

# Reset seed data only, then re-seed
./scripts/seed-heavy-load.ps1 -Clear

# Dry run
./scripts/seed-heavy-load.ps1 -WhatIf
```

**Safety:** the runner aborts unless the database name contains `TaskManagementSystem`, unless you pass `-AllowAnyDatabase`.

After seeding, verify counts in the script output, then re-run GET timing:

```powershell
./scripts/run-get-query-timing.ps1 -BaseUrl http://localhost:61173
```

Clear only (no re-seed): run `002_ClearHeavyLoad.sql` manually or use `-Clear` without re-running seed (the `-Clear` flag runs clear before seed in `seed-heavy-load.ps1`).

## Integration test bootstrap

[`IntegrationTestDatabaseBootstrap`](../tests/TaskManagementSystem.TestCommon/Integration/IntegrationTestDatabaseBootstrap.cs) runs every script in `Scripts/Migrations/` against an isolated `TmsTests_*` database, then [`IntegrationTestDataSeeder`](../tests/TaskManagementSystem.TestCommon/Integration/IntegrationTestDataSeeder.cs) executes `002_IntegrationTestData.sql`. [`TmsWebApplicationFactory`](../tests/TaskManagementSystem.TestCommon/Integration/TmsWebApplicationFactory.cs) drops the database on fixture dispose.

Each SQL batch is executed with `SET QUOTED_IDENTIFIER ON` / `SET ANSI_NULLS ON` (see [`SqlScriptSeeder`](../src/Database/TaskManagementSystem.Database/SqlScriptSeeder.cs)) so filtered indexes in later migrations apply cleanly.
