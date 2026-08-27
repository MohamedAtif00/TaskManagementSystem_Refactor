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

## Rules for evidence

- Count endpoints from `Controllers/`, screens from `src/pages/`, entities from `DbSet<>` in `DataContext.cs` — not from old markdown.
- Authorisation is real only if the **server** rejects the wrong role. UI hiding is Partial.
- Curriculum hierarchy: confirm against current project/subject models and the 2026 curriculum migrations, not the Year → Project → Unit wording in older notes.
- Background processes: find hosted services / workers, do not copy a remembered number.
