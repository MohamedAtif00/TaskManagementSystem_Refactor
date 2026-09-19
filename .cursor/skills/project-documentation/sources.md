# Where to verify facts

Read only what the current document needs. Paths are from repo root.

## Orientation

| Topic | Start here |
|---|---|
| Product intent (may be stale vs `docs/`) | `docs/SUPPORTING_NOTES.md` (section: explanation prompt) |
| Historical screen list | `docs/SUPPORTING_NOTES.md` (count is stale; recount `src/pages`) |
| Task/curriculum ERD notes | `docs/SUPPORTING_NOTES.md` (pre-2026 folder tree; prefer current models) |
| Task Logger | `docs/PRD.md` F-13 and `docs/SUPPORTING_NOTES.md` |
| Sprints | `docs/SUPPORTING_NOTES.md` |

## Backend (`AutomatedTaskSystem/`)

| Topic | Path |
|---|---|
| Composition root | `Program.cs`, `Builder/DependancyInjections.cs` |
| Entities | `Data/DataContext.cs`, `Models/` |
| Roles | `Models/Enums/UserRoleEnum.cs` |
| Task status | `Models/Enums/TaskStatusEnum.cs` |
| HTTP API | `Controllers/` |
| Workflow / tasks | `Services/Task/` |
| Rollback | `Services/Rollback/` |
| Daily report | `Services/DailyReport/` |
| Leave / WFH | `Services/Leave/`, `Services/WorkFromHome/` |
| Auth / sessions | `Services/Auth/`, session-related models/services |
| Real-time | `Hub/UserHub.cs` |
| Schema history | `Migrations/` (eras for the Brief timeline) |

## Frontend (`AutomatedTaskSystem.UI/client/`)

| Topic | Path |
|---|---|
| Routes | `src/pages/` |
| Role navigation | `src/components/sidebar/sidebar.tsx` |
| API client | `src/lib/API/` |
| Auth UI | `src/components/auth/` |
| Role dashboards | pages/components named `home`, `dashboard`, sprint overview |

## Refactored backend (`TaskManagementSystem/`)

| Topic | Path |
|---|---|
| Architecture / module boundaries | `TaskManagementSystem/docs/ARCHITECTURE.md` |
| DbUp migrations / seeds | `TaskManagementSystem/docs/DATABASE.md` |
| Identity User aggregate (profile/team only — no leave balances, no TeamleaderId) | `src/Modules/Identity/.../Domain/User.cs` |
| HR employee balances (runtime source of truth) | `src/Modules/HR/.../Domain/EmployeeBalanceRecord.cs` |
| User create → HR balance seed | `UserCreatedIntegrationEvent` → `OnUserCreatedIntegrationEvent` |
| Default entitlements factory | `EmployeeBalanceRecord.CreateWithDefaultEntitlements` |
| Leave columns dropped from identity.Users | `Scripts/Migrations/021_identity_DropUserLeaveColumns.sql`, `Structure/identity/Tables/Users.sql` |
| Team leader on organization.Teams only | `Scripts/Migrations/022_organization_TeamsTeamleaderId.sql`, `023_identity_DropUserTeamleaderId.sql` |

Leave balances on the **legacy** `AutomatedTaskSystem` `User` model are not authoritative for the refactored modular monolith — verify HR `EmployeeBalanceRecord` and `hr.EmployeeBalances` instead.

## Rules for evidence

- Count endpoints from `Controllers/`, screens from `src/pages/`, entities from `DbSet<>` in `DataContext.cs` — not from old markdown.
- Authorisation is real only if the **server** rejects the wrong role. UI hiding is Partial.
- Curriculum hierarchy: confirm against current project/subject models and the 2026 curriculum migrations, not the Year → Project → Unit wording in older notes.
- Background processes: find hosted services / workers, do not copy a remembered number.
