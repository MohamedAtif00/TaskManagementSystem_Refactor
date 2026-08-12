# Automated Task System (ATS) — System Description Document

| | |
|---|---|
| **System name** | Automated Task System (ATS) / Task Management System v1.0 |
| **Solution file** | `AutomatedTaskSystem.sln` |
| **Document type** | Technical & functional system description |
| **Version** | 1.0 |
| **Date** | 2026-08-11 |
| **Status** | Derived from source code scan of the current working tree |

---

## Table of contents

1. [Executive summary](#1-executive-summary)
2. [Business context and purpose](#2-business-context-and-purpose)
3. [Solution architecture](#3-solution-architecture)
4. [Technology stack](#4-technology-stack)
5. [Domain model](#5-domain-model)
6. [How the system works — core mechanics](#6-how-the-system-works--core-mechanics)
7. [Security, authentication and session management](#7-security-authentication-and-session-management)
8. [Real-time and notification subsystem](#8-real-time-and-notification-subsystem)
9. [Background automation](#9-background-automation)
10. [Covered business use cases](#10-covered-business-use-cases)
11. [Reporting and analytics](#11-reporting-and-analytics)
12. [Application surface (screens and APIs)](#12-application-surface-screens-and-apis)
13. [Gaps, risks and future considerations](#13-gaps-risks-and-future-considerations)
14. [Appendix — reference tables](#14-appendix--reference-tables)

---

## 1. Executive summary

The Automated Task System (ATS) is an internal, full-stack production management platform for **educational curriculum content manufacturing**. It is not a generic task tracker. Its defining characteristic is that work items are **generated automatically by a configurable workflow engine** and are **permanently bound to curriculum entities** (learning objectives) rather than being free-floating tickets.

The system covers four functional pillars in a single deployment:

1. **Curriculum catalogue** — a five-level hierarchy (Academic Year → Project → Term → Subject Group → Subject) beneath which production content is decomposed into Units, Lessons and Learning Objectives.
2. **Workflow automation** — reusable, graph-based *Schemas* (Nodes with prerequisite edges, each containing ordered Steps drawn from a reusable Task Bank). Attaching a Schema to a Learning Objective causes the engine to generate tasks and to keep generating downstream tasks as work is completed.
3. **Execution and delivery** — Kanban boards, spreadsheet views, sprints, priority, pause/resume, flagging, rollback with evidence attachments, work-time capture, and role-scoped visibility.
4. **Workforce administration (HR)** — annual/sick/emergency/unpaid leave, short-duration permissions, work-from-home requests, all with multi-level approval chains, balance enforcement, email and real-time notification, plus full session auditing.

Scale of the implementation as scanned: **27 API controllers** (~200 endpoints), **74 backend service files**, **38 EF Core `DbSet`s across 139 migrations**, **1 SignalR hub with 13 server→client events**, **5 background workers**, and **50 routable frontend pages**.

---

## 2. Business context and purpose

### 2.1 The organisation

The platform serves **Selah El-Telmeez**, an educational content producer. Curriculum content (subjects, units, lessons, learning objectives) is manufactured by specialised internal teams organised into **Groups** that map to production disciplines. Evidence for these disciplines appears directly in the analytics tag labels: *Instructional Design (ID), Subject Matter Expert (SME), Proofreading, Graphic Designer, Voice-Over (VO), Animation, Multimedia Developers, Native Developers, Quality Assurance, Translation*.

### 2.2 Problems the system solves

| Problem before ATS | How ATS addresses it |
|---|---|
| Content production stages were tracked informally; nobody knew which discipline owed what work on which lesson. | Every Learning Objective is attached to a workflow Schema. The engine creates one Task per Step, pre-assigned to the responsible Group. |
| Handoffs between disciplines were manual and error-prone. | Completing the last Step of a Node automatically creates the Tasks of the next Node — but only when *all* prerequisite Nodes are complete. |
| Rework ("send it back to the designer") had no audit trail. | Explicit **Rollback** feature: target Step selection, per-Step issue notes, clarification text, and file attachments, all persisted with history. |
| Effort per stage was unknown, so estimating and staffing were guesswork. | `TaskWorkTimes` records every start/stop with an explicit end reason. The Task Logger scores actual vs. expected duration. |
| Managers at different levels needed different views but the tool showed everyone everything. | Five roles with role-scoped data filtering in the service layer and role-gated navigation in the UI. |
| Leave, permissions and WFH were tracked outside the production system, so capacity planning was blind. | HR requests live in the same database with the same users, approval routing follows the same org hierarchy, and balances are enforced and auto-reset. |
| No accountability for who was logged in and working. | Persisted `UserSessions` with login/logout timestamps, reason codes, IP and user agent; Owner-only session browser with a 24-hour activity storyline and force-logout. |

### 2.3 Primary user personas

| Persona | Role enum | What they do in ATS |
|---|---|---|
| **Owner** | `Owner` (4) | Full oversight. Final approver on HR chains. Only role that can view sessions, force logout, and see member leave balances. |
| **Project Manager / Coordinator** | `ProjectManger` (0) | Designs curriculum hierarchy and workflow schemas, creates projects, assigns staff, uses jump/override powers on tasks. |
| **Section Head** | `SectionHead` (1) | Oversees several Groups via `SectionGroups`. Approves HR requests for those groups. |
| **Team Leader** | `TeamLeader` (2) | Owns one Group's throughput. Receives flagged-task alerts. First-level HR approver for direct reports. Some Task Bank steps are marked `TL` and start pre-queued for leaders. |
| **Member** | `Member` (3) | Production contributor. Claims tasks from the Group backlog, works them, completes or flags or requests rollback. Manages own leave. |

---

## 3. Solution architecture

### 3.1 Physical topology

```
┌──────────────────────────────┐        ┌────────────────────────────────────┐
│  Browser (SPA)               │        │  AutomatedTaskSystem (Web API)     │
│  Next.js 13 Pages Router     │◀──────▶│  ASP.NET Core / net10.0            │
│  React 18 + Redux Toolkit    │  HTTPS │  27 Controllers                    │
│  MUI 5 + Tailwind            │  +     │  Service layer (74 files)          │
│  @microsoft/signalr          │  WSS   │  UserHub (SignalR) at /userhub     │
└──────────────────────────────┘        │  5 hosted background workers       │
             ▲                          └────────────────┬───────────────────┘
             │ served by                                 │ EF Core 10 + Dapper
┌────────────┴─────────────────┐        ┌────────────────▼───────────────────┐
│  AutomatedTaskSystem.UI      │        │  SQL Server                        │
│  ASP.NET Core 7 SPA host     │        │  SystemAdminDB_Test_v1             │
│  SpaProxy (dev) →            │        │  38 DbSets                         │
│  wwwroot static (prod)       │        └────────────────────────────────────┘
│  MapFallbackToFile           │                         │
└──────────────────────────────┘        ┌────────────────▼───────────────────┐
                                        │  SMTP (MailKit)   │  SSRS reports  │
                                        └───────────────────────────────────┘
```

### 3.2 Projects in the solution

| Project | Path | Role |
|---|---|---|
| `AutomatedTaskSystem` | `AutomatedTaskSystem/` | The Web API. Contains all domain logic, EF Core model, migrations, SignalR hub, background workers. Target framework **net10.0**. |
| `AutomatedTaskSystem.UI` | `AutomatedTaskSystem.UI/UI.csproj` | ASP.NET Core 7 host that proxies to `next dev` on port 3000 during development and serves the compiled SPA from `wwwroot` in production. Not an API. |
| React client | `AutomatedTaskSystem.UI/client/` | Next.js 13 SPA, 50 routable pages. |
| `AutomatedTaskSystem.Test` | `AutomatedTaskSystem.Test/` | Test project. Integration tests substitute an EF Core in-memory database named `IntegrationTests`. |

### 3.3 Layering inside the API

The backend follows a conventional three-layer arrangement — **Controller → Service → `DataContext`** — with two deliberate deviations:

- **Dapper + raw SQL** is used for the two heaviest read-only analytics screens (`Services/DailyReport/DailyReportSql.cs`, `Services/TaskLogger/TaskLoggerSql.cs`) because the aggregations span six or seven joins with conditional scoring logic that EF Core would translate poorly.
- **Some controllers query `DataContext` inline** rather than delegating to a service — notably `TeamsController`, `SummaryController`, the Task Bank endpoints inside `SchemaController`, and `TaskController.GetPreviousSteps`. This is technical debt rather than design intent.

There is **no AutoMapper and no FluentValidation**. DTO projection is hand-written; validation is inline in services. Responses are wrapped in `ResponseService<T>` / `BaseResponseService`, which carry `{ data, error, message }`.

### 3.4 Request pipeline order

From `Program.cs`:

1. `UseForwardedHeaders` — honours `X-Forwarded-For` / `X-Forwarded-Proto` so client IP survives the reverse proxy (required for session auditing).
2. `UseStaticFiles`
3. `UseCors("_myAllowSpecificOrigins")`
4. `UseDeveloperExceptionPage` in Development/Testing, otherwise `UseExceptionHandler("/Error")`
5. `UseMiddleware<ErrorLoggingMiddleware>` — writes unhandled exceptions with path, query and verb to `ErrorLogs/error_*.txt`
6. Swagger + Swagger UI — **enabled in all environments**
7. `UseAuthentication` → `UseAuthorization`
8. Startup sequence (skipped in Testing): `MigrateAsync` → `SubjectSchemaRepair` → `CurriculumHierarchyRepair` → `DataSeeder.Seed()`
9. `MapControllers` and `MapHub<UserHub>("/userhub")`

---

## 4. Technology stack

### 4.1 Backend

| Concern | Choice | Version |
|---|---|---|
| Runtime | ASP.NET Core | net10.0 |
| ORM | Entity Framework Core (SQL Server) | 10.0.5 |
| Micro-ORM | Dapper (analytics queries) | 2.1.72 |
| Auth | `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.5 |
| Real-time | SignalR (built in) | — |
| Email | MailKit | 4.15.1 |
| Cron parsing | Cronos | 0.11.1 |
| API docs | Swashbuckle.AspNetCore | 10.1.5 |
| Database | SQL Server, catalogue `SystemAdminDB_Test_v1`, connection retry 5 × 5 s | — |

### 4.2 Frontend

| Concern | Choice | Version |
|---|---|---|
| Framework | Next.js **Pages Router** | 13 |
| UI runtime | React | 18.2 |
| Language | TypeScript (`ignoreBuildErrors: true`) | 5.0 |
| Component library | MUI Material + X-DataGrid + X-Charts + X-DatePickers | 5.13 / 6.4 / 8.2 |
| Utility CSS | Tailwind CSS + Sass modules | 3.2 |
| State | Redux Toolkit + react-redux | 1.8 / 8.0 |
| Data fetching | Native `fetch` through a hand-rolled `src/lib/API/` layer — no React Query / SWR / Axios | — |
| Real-time client | `@microsoft/signalr` (WebSockets only, `skipNegotiation: true`) | 8.0.7 |
| Charts | chart.js + react-chartjs-2 + recharts + MUI X-Charts (four libraries) | — |
| Dates | date-fns + dayjs + react-date-range + MUI pickers (four libraries) | — |
| Export | react-csv, xlsx | — |
| Toasts | react-toastify | 11.0.5 |
| Forms | Custom components only — no react-hook-form / Formik / Zod | — |

---

## 5. Domain model

### 5.1 The three model clusters

The database serves three loosely coupled clusters that meet at `User` and at `Task`:

```
CLUSTER A — Curriculum content                CLUSTER B — Workflow definition
────────────────────────────────              ──────────────────────────────
AcademicYear                                  SchemaType
   └─ CurriculumProject                          └─ Schema
        └─ CurriculumTerm                             └─ Node ──┐ NodeSequences
             └─ SubjectGroup                              │      │ (self M:N,
                  └─ Subject ◀──M:N── User                │      │  prerequisites)
                       └─ Unit                            └─ Step ──┐ RSteps
                            └─ Lesson                          │     │ (self M:N,
                                 └─ LearningObjective ─────────┘     │  rollback
                                      │   (SchemaId)                 │  targets)
                                      │                         TaskBank ── Group
                                      ├─ Task ◀────────── Step ──┘
                                      ├─ Comment
                                      └─ SprintLearningObjective ── Sprint

CLUSTER C — Organisation, HR and audit
──────────────────────────────────────
Section ── SectionGroups ── Group ── User ── Team
   │(Head)                             │
   └─ User                             ├─ LeaveRequest ──┐
                                       ├─ Permission ────┼─ Opinion
                                       ├─ WorkFromHomeRequest ─┘
                                       ├─ RefreshToken
                                       ├─ UserSession
                                       ├─ UserChanges
                                       └─ Notification
```

### 5.2 Curriculum hierarchy (Cluster A)

Five organisational levels above the leaf, then three decomposition levels below it:

| Level | Entity | Table | Parent FK | Delete behaviour |
|---|---|---|---|---|
| 1 | `AcademicYear` | `AcademicYears` | — | — |
| 2 | `CurriculumProject` | `CurriculumProjects` | `YearId` | Restrict |
| 3 | `CurriculumTerm` | `CurriculumTerms` | `ProjectId` | Restrict |
| 4 | `SubjectGroup` | `SubjectGroups` | `TermId` | Restrict |
| 5 | `Subject` | `Subjects` | `SubjectGroupId` | Restrict |
| 6 | `Unit` | `Units` | `SubjectId` | **Cascade** |
| 7 | `Lesson` | `Lessons` | `UnitId` | **Cascade** |
| 8 | `LearningObjective` | `LearningObjectives` | `LessonId` | **Cascade** |
| 9 | `Task` | `Tasks` | `LearningObjectiveId` | **Cascade** |

The split at level 5/6 is meaningful: the upper five levels are **catalogue structure** protected by `Restrict` (you cannot delete a term that still has content), while the lower four are **content decomposition** that cascades. Every entity in the chain also carries an `Archived` flag, and the application prefers soft-archive over hard delete throughout.

`Subject` additionally carries a `Status` of type `ProjectStatusEnum` (`Active` / `Closed` / `Hold` / `Reopened`) which is what the UI exposes as project lifecycle tabs.

**Learning Objective** is the pivotal entity. It carries not only `Name` and `LessonId` but the production metadata that drives everything downstream: `SchemaId` (which workflow applies), `Tag`, `Environment`, `Template`, plus lifecycle timestamps `CreateAt`, `StartedAt`, `DoneAt`.

### 5.3 Workflow definition (Cluster B)

This is the part that makes ATS a workflow engine rather than a task list.

- **`Schema`** — a named, reusable production process, optionally typed via `SchemaType`. Archivable and duplicable.
- **`Node`** — a stage within a Schema. Nodes form a **directed graph**, not a list: the `Next` / `Previous` many-to-many self-relationship (join table `NodeSequences`) expresses prerequisites, so a Node can require several predecessors and can fan out to several successors. `isStart` and `isEnd` mark graph entry and exit; `Order` drives display sequence.
- **`Step`** — an ordered unit of work inside a Node. A Step points at a `TaskBank` entry, and carries its own `Duration` and `Priority`. Steps also have a self many-to-many relationship (join table `RSteps`) defining **which earlier Steps this Step may roll back to** — rollback targets are configured, not arbitrary.
- **`TaskBank`** — the reusable catalogue of work types. Each entry names the work, binds it to the **`Group`** (discipline) that performs it, sets a default `Duration`, a `Type` (`Creation` / `Review`), an `Active` flag, and a **`TL`** flag meaning "this work is performed by the Team Leader".

The consequence: a Schema is a *template of a production line*. Because `TaskBank` carries the `GroupId`, the engine knows the responsible discipline for every generated task without any human routing decision.

### 5.4 Task and its satellites

`Task` is the central operational record.

| Field | Purpose |
|---|---|
| `Status` | `Backlog` / `ToDo` / `Doing` / `Done` / `Rollback` |
| `StepId` | Which workflow Step produced it (nullable — standalone tasks exist) |
| `LearningObjectiveId` | The curriculum item being produced |
| `GroupId` | Owning discipline, inherited from the Task Bank entry |
| `UserId` | Assignee (nullable while in the Group backlog) |
| `Priority` | `None` / `High` / `Medium` / `Low` |
| `Duration` | Expected minutes |
| `Pause` | Work is suspended |
| `Flagged` | Escalated to the Team Leader |
| `Attention` | Needs attention marker |
| `TL` | This is Team-Leader work |
| `IsReview` | Produced by a `Review`-type Task Bank entry |
| `IsRollback`, `RollbackCount` | Rework provenance and counter |
| `FromId` | Self-reference to the originating task (rollback / jump lineage) |
| `CreatedAt` | Generation timestamp |

Four satellite tables give it a complete audit surface:

- **`TaskActivity`** — an append-only event log with **22 event types** (`Created`, `Status_ToDo`, `Status_Doing`, `Status_Done`, `Status_Rollback`, `Pause`, `Resume`, `Flag`, `Unflag`, `Assign`, `Comment`, `Rollback`, `PriorityChange`, `Skip`, `ProcessChange`, `Jump`, `ReactivateJump`, `Reactivated`, `EditComment`, `DeleteComment`, `ProcessChangeCreate`). Carries two optional actor references and two task references, so reassignments and rollbacks record both sides.
- **`TaskWorkTimes`** — start/end/duration per user per work session, with an explicit `EndReason` (`Flag`, `Pause`, `Complete`, `Session`, `Reassign`, `ChangeOfProcess`, `Skip`). This is the source of truth for effort reporting. `Session` as an end reason is what lets the inactivity watchdog close open timers honestly.
- **`Comment`** — threaded via a one-to-one `ChildId` self-reference; scoped to both a Learning Objective and optionally a Task.
- **`Rollback`** → **`RollbackIssue`** (per-Step notes) and **`RollbackAttachment`** (evidence files with name, path, content type, size).

### 5.5 Organisation and HR (Cluster C)

`User` is unusually wide because it carries **both** identity and HR balances:

- Identity and org: `Code` (unique, max 6 characters — this *is* the login credential), `Name`, `Email`, `Phone`, `Title`, `HR_code`, `Role`, `AccountType` (`Internal` / `External`), `TeamId`, `GroupId`, `TeamleaderId` (self-reference), `OnBoard`, `Archived`.
- Leave balances: `Annual_leave` / `Annual_leave_MAX`, `Sick_leave`, `Emergency_leave` / `Emergency_leave_MAX`, `Permission` / `Permission_MAX`, `WorkFromHome` / `WorkFromHome_MAX`, `FromNextBalanceDaysUsed`, `OldAnnualBalance`.

Three request types share one approval mechanism:

| Request | Table | Distinguishing fields |
|---|---|---|
| `LeaveRequest` | `LeaveRequests` | `StartDate`, `EndDate`, `Type` (Annual/Sick/Emergency/UnpaidLeave/FromNextBalance), `MedicalCertificatePath` |
| `Permission` | `Permissions` | `PermissionDate`, `FromTime`/`ToTime` (`TimeOnly`, converted to `TimeSpan`), `Type` (WorkAssignment/EarlyDeparture/LateArrival/Departure) |
| `WorkFromHomeRequest` | `WorkFromHomeRequests` | single `Date` |

All three record `TeamleaderId` and `SectionheadId` at creation time (freezing the approval chain against later org changes) and all three link to **`Opinion`** — one row per approver per request, carrying `IsApproved` and a `Comment`. `Opinion` has three nullable foreign keys, one per request type, which is how a single approval table serves all three flows.

Enum storage note: leave, permission, WFH and notification enums are persisted **as strings**, not integers, which makes the tables human-readable at the cost of index size.

`UserChanges` provides an HR audit trail: who changed which user, what action, a serialised `Changes` payload, and when.

### 5.6 Indexes and performance configuration

| Index | Table | Notes |
|---|---|---|
| `Code` | `Users` | Unique — enforces login-code uniqueness |
| `TaskId` | `DailyReportNoteOverrides` | Unique — one note override per task |
| `RefreshToken` | `UserSessions` | Session lookup by token |
| `(UserId, LogoutAt)` | `UserSessions` | Open-session lookup |
| `(Type, TimeStamp, TaskId)` | `TaskActivities` | Covering index for activity analytics |
| `(Archived, GroupId, CreatedAt)` + includes | `Tasks` | Covering index for board and report queries |
| `(TaskId, Id DESC)` + includes | `Rollbacks` | Latest-rollback lookup |

There are **no EF Core global query filters**; archive filtering is applied explicitly per query.

### 5.7 Schema evolution

139 migrations tell a clear product story:

| Period | Theme |
|---|---|
| 2022 | Core model: users, teams, groups, sections, projects, schemas, first Tasks, JWT + refresh tokens, roles |
| 2023 | Workflow maturity: Task Bank, activity records, work-time durations, pause, rollbacks and rollback targets, schema types, node ordering, project status |
| 2025 H1 | HR module: leave, permissions, opinions, WFH, medical certificates, user change audit |
| 2025 H2 | Sprints linked to learning objectives |
| 2026 | Notifications, leave reset and from-next-balance, the folder → curriculum hierarchy refactor, daily-report overrides and performance indexes, rollback attachments, user sessions |

Two model refactors are visible as archaeology in the codebase: `RootProject` / `ProjectYear` / `ProjectTerm` were replaced by the `AcademicYear` → `Curriculum*` chain in `20260722160000_ReplaceFoldersWithCurriculumHierarchy`, and the intermediate "Folder" model was likewise superseded. The old model classes still exist in `Models/` but are unmapped.

---

## 6. How the system works — core mechanics

### 6.1 Task generation

Tasks are created in three ways:

1. **Automatically at Learning Objective creation.** `LessonService.CreateLO` attaches a Schema and generates Tasks for every Step of the Schema's start Node.
2. **Automatically on completion** — the `CreateNext` mechanism described below.
3. **Manually**, via `POST /tasks` with a Task Bank entry, a Learning Objective and a user. This is the standalone/ad-hoc escape hatch.

Initial status depends on the `TL` flag: Task Bank entries marked `TL` are created directly in **`ToDo`** (queued for the Team Leader), while everything else starts in **`Backlog`** (unclaimed in the Group pool).

### 6.2 The task lifecycle

```
                    ┌─────────── ToggleFlag / Pause (orthogonal) ───────────┐
                    │                                                       │
  ┌──────────┐  proceed   ┌────────┐  proceed   ┌───────┐  complete  ┌──────┐
  │ Backlog  │──────────▶ │  ToDo  │──────────▶ │ Doing │──────────▶ │ Done │
  └──────────┘            └────────┘            └───┬───┘            └──┬───┘
       ▲                                            │                   │
       │                                    rollback│                   │ CreateNext
       │                                            ▼                   ▼
       │                                      ┌──────────┐      new Tasks at next
       └──────── new Task at target Step ─────│ Rollback │      Step or next Node
                                              └──────────┘
```

**`PATCH /tasks/{id}/proceed`** advances one stage. Moving out of `Backlog` claims the task for the caller; entering `Doing` opens a `TaskWorkTimes` row.

**`PATCH /tasks/{id}/complete`** closes the work timer with reason `Complete`, sets `Done`, writes a `Status_Done` activity, then invokes **`CreateNext`**.

### 6.3 `CreateNext` — the automation core

This is the single most important behaviour in the system:

1. If the completed Task's Step has a **subsequent Step in the same Node**, create the Task for that Step. The Node stays in progress.
2. If it was the **last Step of the Node**, look at the Node's `Next` edges. For each successor Node, check whether **every one of its prerequisite Nodes has all its final Steps `Done`**. Only successors whose prerequisites are fully satisfied get their Tasks created.
3. If **no non-`Done` tasks remain anywhere in the Subject**, the Subject's `Status` is set to `Closed` and the Owner receives a `ProjectCompleted` / `ProjectClosed` notification.

Because prerequisite satisfaction is evaluated at completion time, the graph correctly handles fan-in (a stage that waits for several upstream stages) and fan-out (one stage releasing several parallel stages) without any orchestrator or scheduler.

### 6.4 Assignment rules

`POST /tasks/{id}/assign` enforces, in `TaskService.AssignUser`:

- A **`Member` cannot assign tasks** at all.
- The assignee's `GroupId` **must match the task's `GroupId`** — unless the caller is `Owner` or `ProjectManger`, who may cross group boundaries.
- Assignment writes an `Assign` activity recording both actors, closes any open work timer with reason `Reassign`, and triggers a `TaskAssigned` notification (real-time toast plus a persisted notification row).

### 6.5 Rollback (rework)

Rollback is only legal **from `Doing`**. The flow:

1. The worker calls `GET /tasks/previous/{taskId}` to retrieve legal rollback targets — these come from the Step's configured `RSteps` relationships, so the process designer controls where rework can go.
2. `POST /tasks/rollback/{id}` accepts multipart form data: the target Step, per-Step `RollbackIssue` notes, free-text `Clarification`, and up to **5 attachments of 10 MB each** (PDF/Office only, whole request capped at 60 MB).
3. A new Task is created at the target Step with `IsRollback = true`, `FromId` pointing at the originating task, and `RollbackCount` incremented.
4. `GET /tasks/{id}/history` returns the full rollback chain.

### 6.6 Managerial overrides

| Operation | Endpoint | Restriction |
|---|---|---|
| **Skip** a task and advance the workflow | `POST /tasks/{id}/skip` | Closes timer with reason `Skip` |
| **Jump** to an arbitrary Node ahead in the graph | `PUT /tasks/{id}/jump` (targets from `GET /tasks/{id}/jump-points`) | **`Owner` or `ProjectManger` only** |
| **Change the Schema** on a Learning Objective | `PATCH /learning-objectives/{id}` | **PM only.** Can auto-complete tasks up to defined stop steps and fork the LO |
| **Complete another user's task** | `PATCH /tasks/{id}/complete` with force | `Owner` / `ProjectManger` only |
| **Archive a Node or Step** with live tasks | `DELETE /nodes/{id}`, `DELETE /steps/{id}` | Preceded by an `OPTIONS .../delete` safety probe; surviving tasks are migrated or advanced |

The `OPTIONS` pre-delete probe is a notable design choice: before destroying part of a running workflow the UI asks the API what the damage would be, and shows the operator the affected task count.

### 6.7 Pause, flag and the inactivity watchdog

- **Pause** (`PATCH /tasks/{id}/pause`) toggles on `ToDo` and `Doing` tasks, closing the work timer with reason `Pause` and reopening it on resume. This keeps effort data honest across interruptions.
- **Flag** (`PATCH /tasks/{id}/flag`) marks a task as blocked/needing attention and pushes a `TaskFlagged` real-time alert plus a persisted notification to the Team Leader. Work time is closed with reason `Flag`.
- The **inactivity watchdog** in `UserConnectionService` tracks SignalR presence. After **10 minutes** of inactivity it calls `PauseAllTasksForUser` (closing timers with reason `Session`), invalidates the user's tokens, and pushes `ForceLogout`. Without this, a member who walked away would accumulate phantom work time.

### 6.8 Sprints

`Sprint` (name, description, start/end date, `IsArchived`) links to Learning Objectives through the `SprintLearningObjectives` junction table — a many-to-many, so an LO can appear in more than one sprint. Sprints do **not** own tasks; they are a *lens* over the LOs and therefore over the tasks those LOs generate. This is why sprint boards and project boards share the same task card endpoints with different scoping.

### 6.9 HR approval mechanics

**Leave.** Working days are computed **Monday–Friday**; Saturday and Sunday are excluded from all day counts. Type-specific rules:

- **Sick leave over 3 days requires a medical certificate** upload.
- **Emergency leave is blocked** inside a blackout window between `EmergencyBlackoutCutoffDate` and `ResetDate`.
- **FromNextBalance** (borrowing against next year's entitlement) is capped at `FromNextBalanceMaxDays` (default **3**) and is only permitted inside a configured window before the reset date.
- Balances are validated against `Annual_leave_MAX`, `Emergency_leave_MAX`, and pending requests.
- `POST /Leave/preview` lets a user see the balance impact and day split before submitting.

**Approval routing** is by role: a Team Leader gives the first Opinion for direct reports (`TeamleaderId`), Section Heads for users in their section's groups, Project Managers for TL/Member requests, and the **Owner is the final approver** — Owner approval also triggers an email. Each approver may hold only one Opinion per request. `POST /Leave/CreateBulkOpinion` and `POST /Permission/BulkApprove` let managers clear a queue in one action.

**Permissions.** Available slots are `Permission_MAX − (used + pending)`. **Owner permission requests are auto-approved** immediately and an email goes to the CEO.

**Work from home.** No duplicate active request for the same date, no past dates, capped at `WorkFromHome_MAX`.

**Monthly reset.** `MonthlyDatabaseOperationWorker` runs on the Cronos expression `0 0 21 * *` — midnight on the 21st of each month, local time — and resets `Permission = 0` and `WorkFromHome = 0` for **all users**. The 21st (rather than the 1st) implies the payroll month boundary.

---

## 7. Security, authentication and session management

### 7.1 Authentication model

Login is by **6-character user `Code`** submitted to `POST /auth/login`. There is **no password and no external identity provider**. On success:

- A JWT access token is returned in the response body with claims `Id`, `ClaimTypes.Role`, `ClaimTypes.NameIdentifier`, signed HMAC-SHA256 with the key at `AppSetting:Token`.
- A refresh token (64 cryptographically random bytes, base64) is set as an **HttpOnly cookie**.
- A `UserSession` row is opened with `LoginAt`, the refresh token, client IP (resolved through `ClientIpHelper` from `X-Forwarded-For` / `X-Real-IP`) and user agent.

Token validation deliberately sets `ValidateIssuer = false` and `ValidateAudience = false`.

### 7.2 Lifetimes

```csharp
9:10:AutomatedTaskSystem/Services/Token/AuthTokenLifetimes.cs
    public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromMinutes(15);
```

Access **10 minutes**, refresh **15 minutes**. This is aggressive: the effective maximum unbroken session is 15 minutes unless refresh happens. Note that `PROJECT_EXPLANATION_PROMPT.md` states 2 days / 10 days — that document is stale; the code above is authoritative.

### 7.3 Session lifecycle and reasons

`SessionLogoutReason` records *why* every session ended:

| Reason | Triggered by |
|---|---|
| `Manual` | User clicks logout |
| `TokenExpired` | `POST /auth/session-expired` from the client, or the expiry sweeper |
| `InactivityTimeout` | 10-minute SignalR inactivity in `UserConnectionService` |
| `ForcedByAdmin` | `POST /auth/force-logout/{userId}` |
| `ReplacedByNewLogin` | A new login closes prior open sessions for that user |

`SessionExpiryBackgroundService` sweeps every **1 minute** closing sessions whose tokens have passed expiry, so the audit log never shows indefinitely-open sessions.

Sessions are keyed to **login/logout, not SignalR connect/disconnect** — a deliberate choice so that a browser refresh does not manufacture a false session.

### 7.4 Owner-only administration

| Endpoint | Purpose |
|---|---|
| `GET /auth/sessions` | Paged session list, filterable by date range and logout reason (page size capped at 100) |
| `GET /auth/sessions/day?userId=&date=` | 24-hour storyline of active/inactive segments for one user |
| `POST /auth/force-logout/{userId}` | Invalidate tokens, close sessions, push `ForceLogout` over SignalR |

The role check is enforced **in `UserSessionService` / `AuthService`**, not by an authorization attribute.

### 7.5 Client-side session enforcement

`src/lib/Auth.ts` stores the JWT in `localStorage` under `access-token`, parses `exp` locally, and:

- Sets an auto-logout timer for the expiry moment.
- **Re-checks expiry on every route change** and aborts navigation if expired — this closed a real bug where an expired user could keep navigating because auth was only checked on first load.
- Re-checks on tab visibility change.
- Calls `POST /auth/session-expired` so the server can close the session with the correct reason.
- Handles the SignalR `ForceLogout` event by ending the session and dispatching a Redux logout.

There is **no silent client-side refresh** — the refresh cookie exists and the `/auth/refresh-token` endpoint exists, but the SPA never proactively exchanges it.

### 7.6 Other security-relevant configuration

- **CORS** policy `_myAllowSpecificOrigins`: `localhost:3000`, `localhost:8081`, `https://localhost`, `https://localhost:443`, `stdigital.stp.local`, `https://ats.stp.local`; any header, any method, **credentials allowed**.
- **`EncryptionService`** provides AES encrypt/decrypt using `AppSetting:Crypto`.
- **`ErrorLoggingMiddleware`** writes exceptions to `ErrorLogs/error_*.txt`.
- **No rate limiting**, no ASP.NET authorization policies, no `[Authorize(Roles=...)]` attributes anywhere.

---

## 8. Real-time and notification subsystem

### 8.1 The hub

`Hub/UserHub.cs` is mapped at `/userhub`. The client connects WebSockets-only with `skipNegotiation: true`, passing the JWT via the `access_token` query string. Messages target individual users via `Clients.User(userId)` — **no SignalR groups are used**.

Client → server methods:

| Method | Effect |
|---|---|
| `CheckUserDisconnection(userId)` | Replies with `UserDisconnected` status |
| `UpdateUserActivity(userId)` | Refreshes the in-memory activity timestamp that the inactivity watchdog reads |

### 8.2 Server → client events

| Event | Payload meaning | Frontend behaviour |
|---|---|---|
| `OnConnectedMessage` | Pending-approval counts on connect (Owner / Section Head / Team Leader) | Seeds the sidebar pending badge |
| `UpdatePendings` | Pending count changed | Updates badge, toasts new requests |
| `LeaveRequestOpinion` | Leave decision | Approve/reject toast to the requester |
| `PermissionRequestOpinion` | Permission decision | Approve/reject toast |
| `WorkFromHomeOpinion` | WFH decision | Approve/reject toast |
| `TaskAssigned` | New assignment | Toast linking to `/tasks/{projectId}/board?taskId={taskId}` |
| `TaskFlagged` | Task escalated | Rich flag notification toast to the Team Leader |
| `ProjectAssigned` | Project assignment | Toast linking to the project board |
| `ProjectCompleted` | All tasks done | Toast with completion stats (to Owner) |
| `ProjectClosed` | Project auto-closed | Toast with reason (to Owner) |
| `ReceiveError` | Server-side error surfaced live | Error toast |
| `UserDisconnected` | Presence response | Presence display |
| `ForceLogout` | Session terminated by admin, expiry or inactivity | Ends session, Redux logout |

### 8.3 Persistent notifications

Every operationally significant event also writes a `Notification` row so it survives the user being offline:

- `Category`: `General`, `Leaves`, `WorkUpdates`, `Unread`
- `Type`: `Project`, `Sprint`, `Leave`, `Task`, `System`
- `Status` (for actionable items): `Pending`, `Accepted`, `Declined`
- `HasActions` marks a notification the user can act on inline; `POST /notifications/{id}/action` delegates to the Leave, Permission or WFH service to approve or reject **directly from the notification centre**.
- `AdditionalData` holds a JSON blob used for deep-link routing (for example `projectId` for task-assignment notifications) and for classification (a `kind:flagged` marker the flagged filter matches on).
- Time filters of 7, 30 and 90 days are supported on the list endpoint.

The dual-write pattern (SignalR push **and** DB row) means real-time delivery is an optimisation, not a requirement for correctness.

---

## 9. Background automation

| Worker | Schedule | Responsibility |
|---|---|---|
| `SessionExpiryBackgroundService` | every **1 minute** | Close `UserSession` rows whose tokens have expired |
| `UserConnectionService` cleanup timer | every **1 minute** | Detect 10-minute inactivity → pause the user's tasks, invalidate tokens, force logout |
| `LeaveResetBackgroundService` | every **24 hours** | Annual leave reset on the configured `ResetDate`, once per year, journaled in `LeaveResetLogs` |
| `RemoveOldAnnualLeaveServiceBackgroundService` | every **24 hours** | Purge stale annual leave records per `RemoveOldAnnualLeavesDate` |
| `MonthlyDatabaseOperationWorker` | Cronos `0 0 21 * *` (midnight, 21st, local TZ) | Reset `Permission = 0` and `WorkFromHome = 0` for all users |

Additionally, on every non-Testing startup the API runs `MigrateAsync`, then two data-repair routines (`SubjectSchemaRepair`, `CurriculumHierarchyRepair`) that reconcile rows left inconsistent by the hierarchy refactors, then `DataSeeder.Seed()`.

---

## 10. Covered business use cases

### UC-01 Curriculum catalogue management
**Actor:** Project Manager, Owner. **Route:** `/projects`. **API:** `/curriculum/*`.
Create and maintain the five-level catalogue (Academic Year → Curriculum Project → Term → Subject Group → Subject). Parent levels are delete-protected while descendants exist. Any level can be archived. The UI presents this as a drill-down tree.

### UC-02 Content decomposition
**Actor:** Project Manager. **Route:** `/subjects/{subjectId}`. **API:** `/subjects/{id}/units`, `/units/{id}/lessons`, `/lessons/{id}/learning-objective`.
Break a Subject into Units, Lessons and Learning Objectives. Creating an LO requires choosing a workflow Schema; the system immediately generates the first Node's tasks. Archiving a Lesson or Unit cascades to LOs, tasks and comments.

### UC-03 Workflow schema design
**Actor:** Project Manager, Owner. **Route:** `/schemas`, `/schemas/{schemaId}`, `/schemas/task-bank`, `/schemas/archived`.
Build reusable production processes: define Nodes and their prerequisite edges, add ordered Steps, bind each Step to a Task Bank entry (which fixes the responsible Group and default duration), configure legal rollback targets, reorder Nodes and Steps, duplicate a Schema as a starting point for a variant, archive and unarchive. Deleting a Node or Step with live work first runs an `OPTIONS` impact probe.

### UC-04 Task Bank management
**Actor:** Project Manager, Owner. **Route:** `/schemas/task-bank`.
Maintain the catalogue of work types: name, owning Group, default duration, `Creation` vs `Review` type, Team-Leader-performed flag, active flag. This is the point at which production disciplines are encoded.

### UC-05 Project staffing
**Actor:** Project Manager, Owner. **API:** `/subjects/{id}/assign`, `/unassign`, `/users/unassigned`, `/users/assigned`.
Assign users to Subjects (many-to-many via `SubjectUser`). Assignment fires a `ProjectAssigned` notification. `GET /subjects/assignment` returns the caller's own assigned projects.

### UC-06 Automated work generation and progression
**Actor:** the system. **Mechanism:** `TaskService.CreateNext`.
Tasks appear without human routing. TL-flagged work starts in `ToDo`; everything else in the Group `Backlog`. Completing a Step releases the next Step, or — if the Node is finished — the successor Nodes whose prerequisites are all satisfied. When a Subject has no incomplete tasks left, it is auto-`Closed` and the Owner is notified.

### UC-07 Day-to-day task execution
**Actor:** Member, Team Leader. **Route:** `/tasks/{projectId}/board` and `/sheet`.
Claim a task from the backlog, proceed through `ToDo` → `Doing` → `Done`, pause and resume, comment (with threaded replies, edit and delete), change priority, flag for escalation. Every transition writes a `TaskActivity` row and opens or closes a `TaskWorkTimes` row with an explicit end reason. The board and sheet views are interchangeable and the preference persists in `localStorage` under `tasks:view`.

### UC-08 Rework via rollback
**Actor:** Member, Team Leader. **API:** `POST /tasks/rollback/{id}`.
Send work back to a *pre-configured* earlier Step with per-Step issue notes, a clarification, and up to 5 supporting files (10 MB each, PDF/Office only). The new task is marked `IsRollback`, linked to its origin via `FromId`, and the rollback counter increments. Full history is retrievable, and attachments are downloadable.

### UC-09 Managerial workflow intervention
**Actor:** Project Manager, Owner. **API:** `/tasks/{id}/skip`, `/tasks/{id}/jump`, `PATCH /learning-objectives/{id}`.
Skip a task, jump forward to any legal Node ahead of the current position, or change the Schema of a Learning Objective mid-flight — the last of which can auto-complete tasks up to defined stop steps and fork the LO. Restricted to PM/Owner and fully journaled as `Skip`, `Jump`, `ProcessChange` activities.

### UC-10 Sprint planning and delivery
**Actor:** Project Manager, Team Leader. **Route:** `/sprints`, `/sprints/{sprintId}`, `/tasks/sprint/{sprintId}/board`.
Create a time-boxed sprint, attach Learning Objectives to it, then work the sprint as a cross-project board or sheet. Sprints are archivable. Because sprints reference LOs rather than tasks, tasks generated after the sprint opens automatically appear in it.

### UC-11 Role-based landing dashboards
**Actor:** all. **Route:** `/`. **API:** `/dashboards/{project-manager|team-leader|section-head|member}`.
The first screen after login is determined by role. Owner and PM share the PM dashboard (users, projects, schemas, active tasks, LO status charts, group charts); Team Leader sees team throughput; Section Head sees section-scoped task metrics; Member sees personal tasks and leave. Each endpoint validates the caller's role and returns 401 on mismatch.

### UC-12 Workload visibility
**Actor:** PM, Section Head, Team Leader, Owner. **Route:** `/user-tasks`. **API:** `/users/tasks`, `/users/{id}/tasks`.
Per-user `ToDo` / `Doing` counts and backlog totals, used for load balancing and reassignment decisions.

### UC-13 Daily operational report
**Actor:** all roles (role-scoped data). **Route:** `/daily-report`. **API:** `/daily-reports/*`.
A Dapper-backed report over daily task activity, filterable by team, semester, subject, grade, status and problem type, with summary cards, charts and CSV/Excel export. Managers can attach a per-task note override (`PATCH /daily-reports/{taskId}/notes`, one override per task enforced by a unique index).

### UC-14 Productivity measurement (Task Logger)
**Actor:** Owner, PM, Section Head, Team Leader, Member (scoped). **Route:** `/task-logger`. **API:** `/task-logger/*`.
A read-only productivity view with **no manual entry** — everything is derived from existing task and work-time data.

*Actual minutes* = sum of `TaskWorkTimes.Duration` (milliseconds ÷ 60 000) from non-archived users, falling back to `Tasks.Duration`. *Expected minutes* resolves `Steps.Duration` → `TaskBank.Duration` → `Tasks.Duration` → 0.

Scoring:

| Condition | Points |
|---|---|
| No expected duration | 0 |
| Actual **<** expected (before time) | 3 |
| Actual **=** expected (in time) | 2 |
| Actual **>** expected (after time) | 1 |

Report status labels: **Rollback** when `Status = Rollback` or `IsRollback = 1`; **Approved** when `Status = Done`; **Hold** for everything else. Rankings show the top 5 members by points over All Time / This Week / This Month. Rows are excluded if any ancestor entity is archived, if the task is unassigned, or if the assignee is an archived user.

Visibility scoping: Owner and PM see everything; Section Head sees groups under their sections; Team Leader sees their group; Member sees tasks assigned to them or in their group.

### UC-15 Project and sprint analytics
**Actor:** PM, Owner, Team Leader. **Route:** `/tasks/charts/{id}`, `/sprints/charts/{id}`. **API:** `/sprints/analytics/*`, `/subjects/{id}/analytics/*`.
Three symmetric endpoints at both project and sprint scope: `overview`, `learning-objectives-progress`, `learning-objectives-table`. `timePeriod` accepts `1` = Today, `2` = Last Week, `3` = Last Month, `4` = All Time (default).

Semantics: an LO is **completed** when it has tasks and zero not-done tasks; **not started** when it has no tasks but expected steps exist, or all tasks are still in `Backlog`; otherwise **in process**. The "tags" distribution counts **non-done tasks grouped by Group** (using the group's name and colour), which is what makes it read as a discipline-workload chart. All analytics SQL excludes archived entities and — a hard-coded content convention — excludes LOs whose name contains `"old"` (case-insensitive).

### UC-16 Hierarchical reporting
**Actor:** PM, Owner. **Route:** `/project-overview`, `/summaries`, `/advancedReport`.
Date-ranged project reports (`/subjects/reports?start&end`), per-project unit/lesson/LO breakdowns, non-actionable task summaries, and an embedded SSRS report surfaced through an iframe proxy.

### UC-17 Organisation structure administration
**Actor:** PM, Owner. **Route:** `/resources`, `/resources/groups`, `/resources/users`, `/resources/sections`.
CRUD for Groups (name, colour code — the colour propagates into analytics charts), Teams (with user assignment), Sections (with a Head and a set of Groups), and Users (identity, role, group, team, team leader, leave entitlement ceilings). User deletion is an archive. Every user modification is journaled in `UserChanges` and viewable at `/resources/users/{userId}`.

### UC-18 Leave management
**Actor:** all (requester), TL/SH/PM/Owner (approver). **Route:** `/myleave`, `/calendar`, `/members-leaves`.
Request Annual, Sick, Emergency, Unpaid or FromNextBalance leave. Working days count Monday–Friday only. Sick leave over 3 days requires a medical certificate upload, retrievable via `GET /Leave/medical-certificate/{id}`. Emergency leave is blocked during the configured blackout window. FromNextBalance is capped (default 3 days) and time-windowed. `POST /Leave/preview` shows the balance impact before submission. Requests can be cancelled while pending. Approval runs the multi-level Opinion chain with Owner as final approver (which also sends an email), and managers can clear queues with bulk opinions. Owner-only `/members-leaves` shows the whole workforce's balances.

### UC-19 Short-duration permission management
**Actor:** all (requester), managers (approver). **Route:** `/myleave`, `/calendar/permission/{id}`.
Request a timed permission — `WorkAssignment`, `EarlyDeparture`, `LateArrival` or `Departure` — with a date and from/to times. Available slots are `Permission_MAX − (used + pending)`. **Owner requests are auto-approved** and notify the CEO by email. Single and bulk approval are both supported. Counters reset monthly on the 21st.

### UC-20 Work-from-home management
**Actor:** all (requester), managers (approver). **Route:** `/myleave`, `/calendar/workFromHome/{id}`.
Request WFH for a specific date. Duplicate active requests for the same date and past dates are rejected; the total is capped at `WorkFromHome_MAX`. Same Opinion approval chain. Counters reset monthly on the 21st.

### UC-21 Notification centre
**Actor:** all. **Route:** `/notifications`.
A paginated inbox with category and time-range filters, unread counting, mark-one and mark-all read, deep-link routing derived from `AdditionalData`, and **inline approve/reject** for actionable HR notifications.

### UC-22 Session audit and access control
**Actor:** Owner. **Route:** `/sessions`, `/sessions/{userId}`.
Browse all login sessions with filters on date range and logout reason; open a single user's 24-hour activity storyline showing active and inactive segments; force-logout a user, which invalidates tokens, closes the session as `ForcedByAdmin`, and pushes `ForceLogout` in real time.

---

## 11. Reporting and analytics

Five distinct reporting surfaces exist, differing in implementation strategy:

| Surface | Route | Implementation | Purpose |
|---|---|---|---|
| Role dashboards | `/` | EF Core aggregates in `DashboardService` | Landing-page situational awareness |
| Project / sprint analytics | `/tasks/charts/*`, `/sprints/charts/*` | EF Core + raw SQL in the analytics services | LO progress, status distribution, discipline workload |
| Daily Report | `/daily-report` | **Dapper** (`DailyReportSql.cs`) | Daily operational problem tracking with editable notes |
| Task Logger | `/task-logger` | **Dapper** (`TaskLoggerSql.cs`) | Effort vs. estimate scoring and member rankings |
| Hierarchical reports | `/project-overview`, `/summaries`, `/advancedReport` | EF Core; SSRS via iframe proxy | Management and external reporting |

Export is client-side via `react-csv` and `xlsx`, with server-side export button support in `components/button/serverExportButtonProps`.

---

## 12. Application surface (screens and APIs)

### 12.1 Frontend module map

| Module | Routes | Access |
|---|---|---|
| Dashboard | `/` | All (role-differentiated) |
| Projects & curriculum | `/projects`, `/projects/[rootProjectId]`, `/projects/[rootProjectId]/folders/[folderId]`, `/subjects/[subjectId]` | Roles 0, 4 |
| Workflow schemas | `/schemas`, `/schemas/[schemaId]`, `/schemas/archived`, `/schemas/task-bank` | Roles 0, 4 |
| Kanban / tasks | `/tasks`, `/tasks/browse/*`, `/tasks/[projectId]/board`, `/tasks/[projectId]/sheet`, `/tasks/charts/*` | All |
| Sprints | `/sprints`, `/sprints/[sprintId]`, `/sprints/[sprintId]/[learningObjectId]/{board,sheet}`, `/tasks/sprint/[sprintId]/{board,sheet}`, `/sprints/charts/*` | All |
| Resources | `/resources`, `/resources/groups`, `/resources/users`, `/resources/users/[userId]`, `/resources/sections`, `/resources/sections/[sectionId]` | Roles 0, 4 |
| Leaves | `/myleave`, `/calendar`, `/calendar/vacancy/[id]`, `/calendar/permission/[id]`, `/calendar/workFromHome/[id]`, `/members-leaves` | Role-dependent |
| Operations | `/daily-report`, `/task-logger`, `/user-tasks` | All / managers |
| Notifications | `/notifications` | All |
| Sessions | `/sessions`, `/sessions/[userId]` | Role 4 only |
| Reports | `/project-overview`, `/summaries`, `/advancedReport` | **Sidebar links commented out** |

**50 routable pages** in total, several of which are legacy redirects preserved after the hierarchy refactor (`/subjects` → `/projects`, old year/term paths → `/projects/[rootProjectId]`, `/projects/charts/:id` → `/subjects/charts/:id` → `/tasks/charts/:id`).

### 12.2 Sidebar visibility rules

| Item | Visible to |
|---|---|
| Dashboard, Sprints, Kanban, Daily Report, Task Logger, Notifications, Logout | All authenticated |
| User Management, Work Flow, Projects | Role 0 (PM) or 4 (Owner) |
| Sessions, Members Leaves | Role 4 (Owner) only |
| User Tasks | Roles 0, 1, 2, 4 |
| My Leaves | All (standalone for Members, inside the Leaves group for others) |
| Calendar | All except Member (role 3) |

### 12.3 API controller inventory

| Controller | Prefix | Endpoints |
|---|---|---|
| `AuthController` | `auth` | 8 |
| `TaskController` | `tasks` | 26 |
| `NotificationController` | `notifications` | 5 |
| `LeaveController` | `Leave` | 14 |
| `PermissionController` | `Permission` | 9 |
| `WorkFromHomeController` | `WorkFromHome` | 7 |
| `UserController` | `users` | 9 |
| `SubjectController` | `subjects` | 17 |
| `SchemaController` | `schemas` | ~14 |
| `NodeController` | `nodes` | 7 |
| `StepController` | `steps` | ~11 |
| `LearningObjectiveController` | `learning-objectives` | 2 |
| `LessonController` | `lessons` | 3 |
| `UnitController` | `units` | 3 |
| `CurriculumController` | `curriculum` | ~17 |
| `SprintController` | `Sprint` | 5 |
| `SprintAnalyticsController` | `sprints/analytics` | 3 |
| `ProjectAnalyticsController` | `subjects/{subjectId}/analytics` | 3 |
| `DashboardController` | `dashboards` | 4 |
| `DailyReportController` | `daily-reports` | 6 |
| `TaskLoggerController` | `task-logger` | 3 |
| `GroupsController` | `groups` | ~6 |
| `SectionController` | `sections` | ~5 |
| `TeamsController` | `teams` | ~6 |
| `ReportController` | `subjects` | 2 |
| `SummaryController` | `subjects` | 1 |
| `VacationController` | `api/Vacation` | 4 (legacy leave alias) |

---

## 13. Gaps, risks and future considerations

Findings are grouped by severity. Each entry states the observation and the suggested direction.

### 13.1 Critical — security

**G-01 · Authorization is enforced inconsistently and mostly not at the framework level.**
There are **no `[Authorize(Roles=...)]` attributes and no authorization policies anywhere** in the solution. Many controllers carry no `[Authorize]` at all — `SubjectController` (most endpoints), `SchemaController`, `NodeController`, `StepController`, `CurriculumController`, `GroupsController`, `TeamsController`, `UserController`, `SprintController`, and the analytics controllers are open at the controller layer. Role checks live inside service methods (`AuthService`, `UserSessionService`, `DashboardService`, `TaskService`), which means any endpoint whose service forgot the check is effectively public. Notably `POST /users`, `PATCH /users/{id}` and `DELETE /users/{id}` have no controller-level authorization.
*Direction:* add a global `AuthorizationOptions.FallbackPolicy` requiring an authenticated user, then define named role policies (`RequireOwner`, `RequireManager`) and apply them at the controller level. Treat service-level checks as defence in depth rather than the primary gate.

**G-02 · Login is a 6-character code with no second factor and no lockout.**
The credential space is small, the code is stored in plaintext in `Users.Code`, there is no rate limiting anywhere in the pipeline, and no failed-attempt lockout. Brute-forcing `POST /auth/login` is feasible.
*Direction:* at minimum add ASP.NET Core rate limiting on the auth endpoints plus failed-attempt lockout. Strategically, move to a real credential or SSO/OIDC integration and keep the code as a display identifier only.

**G-03 · JWT validation disables issuer and audience checks.**
`ValidateIssuer = false` and `ValidateAudience = false` mean any token signed with the shared key is accepted regardless of origin.
*Direction:* set and validate issuer and audience.

**G-04 · Secrets are in `appsettings.json`.**
`AppSetting:Token` (JWT signing key), `AppSetting:Crypto` (AES key), the SQL connection string and SMTP credentials are committed configuration.
*Direction:* move to user-secrets in development and a secret store (Azure Key Vault / environment variables) in production; rotate the currently committed keys.

**G-05 · Swagger UI is enabled in every environment.**
`UseSwagger` / `UseSwaggerUI` are unconditional, so the full API surface is discoverable in production — which compounds G-01.
*Direction:* gate behind `app.Environment.IsDevelopment()` or an authenticated policy.

**G-06 · Uploaded files are served from `wwwroot` with no authorization.**
Rollback attachments (`GET /tasks/rollback/attachment/{attachmentId}`) and medical certificates are stored under `wwwroot` and the attachment endpoint has no `[Authorize]`. **Medical certificates are health data.**
*Direction:* move uploads outside the web root, serve them only through an authorized endpoint that verifies the caller's relationship to the record, and consider encryption at rest for medical certificates.

**G-07 · The access token is stored in `localStorage`.**
This makes it readable by any injected script (XSS → token theft). The refresh token is correctly HttpOnly; the access token is not.
*Direction:* keep the access token in memory only, or move fully to HttpOnly cookies with CSRF protection.

### 13.2 High — correctness and operability

**G-08 · The API base URL is hard-coded to `localhost:5238`.**

```17:18:AutomatedTaskSystem.UI/client/src/lib/API/index.ts
export const url = "http://localhost:5238";
//  export const url = process.env.REACT_APP_API_URL || "/api";
```

The environment-driven alternative is present but commented out, so a production build points at the developer's machine.
*Direction:* switch to the env-based form and supply the value at build time.

**G-09 · No silent token refresh, with 10-minute access tokens.**
`/auth/refresh-token` exists but the SPA never calls it, so a user is forcibly logged out every 10 minutes (15 at the outside) regardless of activity. For a system whose users spend hours on Kanban boards, this is a serious usability defect and likely drives users to keep re-logging in.
*Direction:* implement a refresh interceptor in the API layer that transparently exchanges the refresh cookie on 401, and/or raise the access-token lifetime to something proportionate (e.g. 60 minutes) now that inactivity handling exists independently.

**G-10 · `typescript.ignoreBuildErrors: true`.**
Type errors do not fail the build, so the type system provides no guarantee at release time.
*Direction:* fix outstanding errors and set this to `false`, or at minimum run `tsc --noEmit` in CI as a blocking gate.

**G-11 · Authorization header attachment is inconsistent in the API client.**
Newer modules (`dashboard`, `sprints`, `sessions`, `notifications`, `dailyReport`, `taskLogger`) always send `authHeader()`; many `PROJECTS`, `RESOURCES` and `SCHEMAS` calls send no auth header at all. This is the client-side mirror of G-01 and means tightening the server will break those screens.
*Direction:* centralise all requests through a single `fetch` wrapper that attaches auth, handles 401 refresh, and normalises errors — then fix G-01 and G-09 in one place.

**G-12 · Stub endpoints return success-shaped responses.**
`GET /Leave/{id}` returns a placeholder `{ message }`, `PUT /Leave/{id}` returns 204 without doing anything, `DELETE /Leave/{id}` returns 204 without deleting, and `DELETE /WorkFromHome/{id}` returns 501. A caller cannot distinguish "updated" from "silently ignored".
*Direction:* implement or return 501 consistently; remove the misleading 204s.

**G-13 · `VacationController` duplicates leave functionality under a different route.**
`api/Vacation` and `Leave` both delegate to `ILeaveRequestService` with different contracts and different validation exposure.
*Direction:* deprecate and remove `VacationController` once no client depends on it.

**G-14 · Two controllers share the `subjects` route prefix.**
`SubjectController`, `ReportController` and `SummaryController` all live under `subjects`, which makes routing fragile and the API hard to reason about.
*Direction:* give reports and summaries their own prefixes.

**G-15 · Hard-coded content convention in analytics SQL.**
Analytics queries exclude Learning Objectives whose **name contains the substring `"old"`** (case-insensitive). This silently drops legitimate content — an LO named "Gold coins" or "Household items" would vanish from every chart.
*Direction:* replace with an explicit flag (`IsDeprecated` / reuse `Archived`) and migrate existing data.

**G-16 · Business rules are hard-coded rather than configured.**
Token lifetimes (10/15 min), inactivity timeout (10 min), attachment limits (5 × 10 MB, 60 MB request), the Monday–Friday working week, the 3-day sick-certificate threshold, the Task Logger 3/2/1 point scale, and the monthly reset day (21st) are all compiled constants. `LeaveSettings` is configurable; nothing else is.
*Direction:* move operational thresholds into configuration or a settings table so HR and operations can change them without a deployment.

### 13.3 Medium — data model and maintainability

**G-17 · Unmapped and dead model classes remain in `Models/`.**
`RootProject`, `ProjectYear`, `ProjectTerm` (superseded by the curriculum chain), `NodeDependency`, `HolyDays` (a migration exists with a matching name but the class was never mapped), and `Assignment.cs` (an **empty file**). `Migrations1/` and `Migrations2/` folders are excluded from compilation, as is `TaskContextModelSnapshot.cs`.
*Direction:* delete dead code; it currently misleads anyone reading the model.

**G-18 · `Team.TeamLeaderId` is a field, not a property, so it is not persisted.**
The intent to model a team leader on `Team` exists in the code but silently does nothing. Team leadership is actually expressed via `User.TeamleaderId`.
*Direction:* remove the fields or promote them to mapped properties, and decide which representation is canonical.

**G-19 · `DailyReportNoteOverride` has divergent column naming.**
The model declares `UpdatedByUserId` while the EF snapshot carries both `UpdatedById` (shadow FK) and `UpdatedByUserId`.
*Direction:* reconcile with an explicit mapping and a cleanup migration.

**G-20 · `User` conflates identity, org placement and HR balances.**
Sixteen leave-balance columns sit on the same row as authentication and org data, so any HR balance change rewrites the identity row and every user query drags HR data along.
*Direction:* extract a `UserLeaveBalance` (or year-scoped `UserEntitlement`) table. This also naturally supports historical balances, which the current design cannot represent.

**G-21 · The `Opinion` table uses three mutually exclusive nullable foreign keys.**
`LeaveRequestId`, `PermissionId` and `WorkFromHomeRequestId` are all nullable with no constraint that exactly one is set.
*Direction:* add a check constraint, or model an `ApprovalRequest` supertype that all three request types reference.

**G-22 · Two overlapping curriculum models are documented.**
`STRUCTURE 1.md` describes a Season → Program → Term → Family → Subject "Subject Library"; the implemented model is AcademicYear → CurriculumProject → CurriculumTerm → SubjectGroup → Subject. The former is not implemented.
*Direction:* either mark `STRUCTURE 1.md` as a superseded proposal or reconcile the vocabulary. Right now new team members receive contradictory information.

**G-23 · Documentation contains stale facts.**
`PROJECT_EXPLANATION_PROMPT.md` claims 2-day access and 10-day refresh tokens (actual: 10 and 15 minutes). `SPRINT_OVERVIEW_API_REQUIREMENTS.md` specifies `GET /sprints/{sprintId}/analytics/overview` while the implementation is `GET /sprints/analytics/overview?sprintId=`. `FRONTEND_SCREENS_LIST.md` claims 38 screens and omits Task Logger and Sessions (actual: 50 routable pages).
*Direction:* stamp documents with a "verified against commit" line and correct the discrepancies.

**G-24 · No AutoMapper, no validation framework.**
Every DTO projection is hand-written across 102 DTO files, and validation is inline in services with no consistent error contract.
*Direction:* introduce FluentValidation for request validation at minimum; DTO mapping can stay manual if that is a deliberate performance choice, but it should be a stated one.

**G-25 · Inline `DataContext` access in controllers.**
`TeamsController`, `SummaryController`, the Task Bank endpoints in `SchemaController`, and `TaskController.GetPreviousSteps` bypass the service layer, so their business rules cannot be reused or unit-tested.
*Direction:* extract to services for consistency.

### 13.4 Medium — functional gaps

**G-26 · Section Head and Member dashboards are documented as incomplete.**
`SPRINT_Role_Based_Home_Page.md` explicitly marks `GET /dashboards/section-head` and `GET /dashboards/member` plus their UI components as not built, while `SPRINT_DIAGRAMS.md` depicts them as done. The endpoints and DTOs now exist in code, so the remaining question is whether the content is complete and correct.
*Direction:* verify against the acceptance criteria in the sprint document and close it out.

**G-27 · `sprintSummary` duplicates `taskSummary`.**
The analytics contract exposes two summary objects but currently returns the same aggregation twice.
*Direction:* either implement the distinct sprint-level aggregation the contract implies, or remove the redundant field.

**G-28 · The Reports module is built but not reachable.**
`/project-overview`, `/summaries` and `/advancedReport` exist and work, but their sidebar navigation entries are commented out.
*Direction:* decide whether these are deprecated (delete them) or hidden pending work (restore navigation).

**G-29 · No i18n despite Arabic-language content and partial RTL.**
There is no i18n framework; the UI is English-only, yet `pages/myleave/index.tsx` applies `dir="rtl"` to modals, implying Arabic input. For an Arabic-market educational publisher this is a notable gap.
*Direction:* introduce `next-intl` or `next-i18next` and a global `dir` strategy if Arabic UI is required.

**G-30 · No theming or accessibility layer.**
No MUI `ThemeProvider`, no dark mode, no custom Tailwind palette, and no evidence of accessibility testing. Colours come from `Group.ColorCode` values entered by administrators, so contrast is unmanaged.
*Direction:* add a theme provider with a defined token set; validate group colours for contrast.

**G-31 · Four charting libraries and four date libraries coexist.**
chart.js, react-chartjs-2, recharts and MUI X-Charts; date-fns, dayjs, react-date-range and MUI date pickers. This inflates the bundle and fragments visual style.
*Direction:* standardise on one of each and remove the rest.

**G-32 · No client-side data-fetching cache.**
All fetching is bare `fetch` with no React Query / SWR, so there is no request deduplication, caching, retry or background revalidation. The existence of a `.../tasks/cards/stream` streaming endpoint suggests board payloads are already large enough to hurt.
*Direction:* adopt a query cache; it would also give a single place to solve G-09 and G-11.

### 13.5 Lower priority — engineering practice

**G-33 · Test coverage appears minimal.**
One test project exists using an in-memory database. Given that `CreateNext` graph traversal, leave balance arithmetic, working-day calculation and Task Logger scoring are the highest-risk logic in the system, they warrant dedicated unit tests.
*Direction:* prioritise tests for `TaskService.CreateNext`, `LeaveRequestService` balance and window rules, and the Task Logger scoring function.

**G-34 · No structured logging or observability.**
Logging writes plain text to `ErrorLogs/error_*.txt`. There is no correlation ID, no structured logging (Serilog), no health check endpoint, and no metrics.
*Direction:* add Serilog with structured sinks, a correlation ID, `/health` endpoints, and basic metrics on the background workers.

**G-35 · Startup runs migrations and data-repair routines automatically.**
`MigrateAsync` plus `SubjectSchemaRepair` and `CurriculumHierarchyRepair` execute on every non-Testing boot. In a multi-instance deployment this races, and repair routines that were needed once for a historical refactor now run forever.
*Direction:* move migrations to a deployment step and retire the repair routines once the data is known good.

**G-36 · In-memory presence state prevents horizontal scaling.**
`UserConnectionService` is a singleton holding connection and activity state in memory, and SignalR has no backplane configured. Two API instances would each have a partial view of presence, breaking the inactivity watchdog and force-logout.
*Direction:* add a SignalR Redis backplane and externalise presence state before scaling out.

**G-37 · Naming and typo debt.**
`ProjectManger` (enum member), `DependancyInjections.cs`, `Resouces.ts`, `StepContoller.cs`, and route-casing inconsistency across the API (`/tasks` vs `/Leave` vs `/Sprint` vs `/WorkFromHome` vs `/api/Vacation`).
*Direction:* fix in a coordinated rename; the enum member is a breaking change worth doing deliberately.

**G-38 · No pagination on several list endpoints.**
`GET /users` and `GET /subjects` return complete collections. `PageList` exists and is used for HR and reporting endpoints but not universally.
*Direction:* apply pagination consistently.

**G-39 · No soft-delete abstraction.**
`Archived` flags are filtered manually in every query, so a single omission silently leaks archived data into a report.
*Direction:* use EF Core global query filters with an explicit `IgnoreQueryFilters()` opt-out.

### 13.6 Strategic considerations

| Consideration | Rationale |
|---|---|
| **Workflow versioning** | Schemas can be edited while work is in flight, and `PATCH /learning-objectives/{id}` can change an LO's Schema mid-production. There is no Schema version pinned to a Learning Objective, so historical reporting cannot reconstruct the process a completed item actually followed. Introducing immutable Schema versions would make audit and throughput comparison sound. |
| **Capacity planning** | The system already holds expected durations, actual work times, group membership, and approved leave/WFH/permission calendars. Combining them would yield genuine forward capacity forecasting — arguably the highest-value feature not yet built. |
| **SLA and ageing** | Tasks record `CreatedAt` and expected `Duration` but nothing escalates automatically when a task sits in `Backlog` too long. Flagging is entirely manual. Automatic ageing alerts would convert the workflow engine from reactive to proactive. |
| **Estimation feedback loop** | Task Logger compares actual against expected but nothing feeds the observed averages back into `TaskBank.Duration` or `Step.Duration`. A periodic recalibration would improve every downstream estimate. |
| **External collaboration** | `AccountType` distinguishes `Internal` from `External`, implying freelance or vendor contributors, but there is no separate access surface, scoped permissions, or reduced-visibility experience for them. |
| **Mobile and approvals** | HR approval is the most time-sensitive, least screen-hungry workflow in the system and the most likely to be done away from a desk. A mobile-friendly approval surface (or email-actionable approvals, since SMTP is already wired) would have outsized impact. |
| **Deployment maturity** | No Dockerfile, CI configuration, or infrastructure-as-code was found. Deployment appears to rely on `dotnet publish` with the IIS SPA build step. Containerising and adding a CI pipeline would let the type-check, test and migration gates above be enforced. |

---

## 14. Appendix — reference tables

### 14.1 Roles

| Enum member | Value | Common name |
|---|---|---|
| `ProjectManger` | 0 | Project Manager / Coordinator |
| `SectionHead` | 1 | Section Head |
| `TeamLeader` | 2 | Team Leader |
| `Member` | 3 | Member (DB default) |
| `Owner` | 4 | Owner |

### 14.2 Task status

| Member | Value |
|---|---|
| `Backlog` | 0 |
| `ToDo` | 1 |
| `Doing` | 2 |
| `Done` | 3 |
| `Rollback` | 4 |

### 14.3 Work-time end reasons

`Flag` (0), `Pause` (1), `Complete` (2), `Session` (3), `Reassign` (4), `ChangeOfProcess` (5), `Skip` (6)

### 14.4 Project / subject status

`Active` (0, default), `Closed` (1), `Hold` (2), `Reopened` (3)

### 14.5 Leave, permission and WFH enums

| Enum | Members |
|---|---|
| `LeaveRequestType` | Annual, Sick, Emergency, UnpaidLeave, FromNextBalance |
| `LeaveRequestStatusEnum` | Pending, Approved, Rejected, Cancelled |
| `PermissionType` | WorkAssignment, EarlyDeparture, LateArrival, Departure |
| `PermissionStatusEnum` | Pending, Approved, Rejected, Cancelled |
| `WorkFromHomeStatusEnum` | Pending, Approved, Rejected, Cancelled |

### 14.6 Notification enums

| Enum | Members |
|---|---|
| `NotificationCategoryEnum` | General, Leaves, WorkUpdates, Unread |
| `NotificationTypeEnum` | Project, Sprint, Leave, Task, System |
| `NotificationStatusEnum` | Pending, Accepted, Declined |

### 14.7 Session logout reasons

`Manual` (0), `TokenExpired` (1), `InactivityTimeout` (2), `ForcedByAdmin` (3), `ReplacedByNewLogin` (4)

### 14.8 Key configuration keys

| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection (retry 5 × 5 s) |
| `AppSetting:Token` | JWT signing key |
| `AppSetting:Crypto` | AES key for `EncryptionService` |
| `EmailSettings` | SMTP host, port, credentials |
| `EmailRecipients` | Standing recipients (CEO, Section Head CC) |
| `LeaveSettings` | `ResetDate`, `EmergencyBlackoutCutoffDate`, `RemoveOldAnnualLeavesDate`, `FromNextBalanceMaxDays` |

### 14.9 Hard-coded operational constants

| Constant | Value | Location |
|---|---|---|
| Access token lifetime | 10 minutes | `Services/Token/AuthTokenLifetimes.cs` |
| Refresh token lifetime | 15 minutes | `Services/Token/AuthTokenLifetimes.cs` |
| Inactivity timeout | 10 minutes | `Services/UserConnectionService.cs` |
| Session sweeper interval | 1 minute | `Services/SessionTracking/SessionExpiryBackgroundService.cs` |
| Rollback request cap | 60 MB | `Controllers/TaskController.cs` |
| Rollback attachments | 5 files × 10 MB, PDF/Office only | `Helper/RollbackAttachmentHelper.cs` |
| Sick certificate threshold | more than 3 days | `Services/Leave/LeaveRequestService.cs` |
| Working week | Monday–Friday | `Helper/LeaveRequestHelper.cs` |
| From-next-balance cap | 3 days (default) | `LeaveSettings` |
| Monthly counter reset | `0 0 21 * *` | `Services/MonthlyDatabaseOperationWorker.cs` |
| Session page size cap | 100 | `Services/SessionTracking/UserSessionService.cs` |
| Task Logger scoring | 3 / 2 / 1 points | `Services/TaskLogger/TaskLoggerSql.cs` |

---

*Document produced from a full scan of the working tree. Where existing project documentation conflicted with source code, the source code was treated as authoritative and the conflict is recorded in G-23.*
