# Product Requirements Document (PRD)
## Automated Task System (ATS) — Task Management System v1.0

| Field | Value |
|---|---|
| **Document type** | Product Requirements Document |
| **System** | Automated Task System (ATS) / Task Management System v1.0 |
| **Organisation** | Selah El-Telmeez — Educational Content Production |
| **Version** | 1.1 |
| **Date** | 2026-08-26 |
| **Status** | For review |
| **Companion documents** | `docs/PROJECT_BRIEF.md`, `docs/BRD.md`, `docs/SYSTEM_DESCRIPTION.md` |

### Document control

| Version | Date | Change summary |
|---|---|---|
| 1.0 | 2026-08-17 | Initial PRD reconstructed from the delivered system |
| 1.1 | 2026-08-26 | Gap review vs code: rollback problem type, User Tasks, HR calendar, LO progress, extra daily-report filters, dashboards delivered, session audit not present, token lifetimes corrected |

This PRD describes the **product the organisation is operating today**, plus the requirements that are specified but not yet met. Where older project notes conflict with source code, the source code is authoritative.

Priority: **M** = Must have · **S** = Should have · **C** = Could have  
Status: **Delivered** · **Partial** · **Not delivered**

---

## Table of contents

1. [Product summary](#1-product-summary)
2. [Features](#2-features)
3. [User requirements](#3-user-requirements)
4. [Functional requirements](#4-functional-requirements)
5. [Constraints](#5-constraints)
6. [Acceptance criteria](#6-acceptance-criteria)
7. [Traceability](#7-traceability)

---

## 1. Product summary

ATS is an internal production-management platform. Project Managers define reusable **workflow schemas**. When a **learning objective** is created and bound to a schema, the engine generates tasks for the starting stage and continues generating downstream tasks as work completes, only when every prerequisite stage is finished.

Users claim and execute work on Kanban boards or spreadsheet views, optionally inside **sprints**. Rework follows pre-configured rollback paths with evidence. Managers see role-scoped dashboards and reports. The same users request leave, permission, and work-from-home through a multi-level approval chain, with real-time and persistent notification.

---

## 2. Features

### 2.1 Feature map

| ID | Feature | Priority | Status | Primary users |
|---|---|---|---|---|
| F-01 | Curriculum catalogue | M | Delivered | PM, Owner |
| F-02 | Content decomposition (Unit / Lesson / Learning Objective) | M | Delivered | PM, Owner |
| F-03 | Workflow schema designer | M | Delivered | PM, Owner |
| F-04 | Task Bank | M | Delivered | PM, Owner |
| F-05 | Automated task generation and progression | M | Delivered | System + all production roles |
| F-06 | Task execution (board and sheet) | M | Delivered | Member, Team Leader |
| F-07 | Assignment and workload | M | Delivered | PM, Section Head, Team Leader, Owner |
| F-08 | Rework (rollback) with evidence | M | Delivered | Member, Team Leader |
| F-09 | Managerial intervention (skip, jump, change process) | M | Delivered | PM, Owner |
| F-10 | Sprint planning and sprint boards | M | Delivered | PM, Team Leader, Owner |
| F-11 | Role-based dashboards | M | Delivered | All five roles |
| F-12 | Daily operational report | M | Delivered | All (scoped) |
| F-13 | Productivity measurement (Task Logger) | M | Delivered | All (scoped) |
| F-14 | Project and sprint analytics | M | Delivered | PM, Owner, Team Leader |
| F-15 | Hierarchical / SSRS reporting | S | Partial | PM, Owner |
| F-16 | Organisation administration | M | Delivered | PM, Owner |
| F-17 | Leave management | M | Delivered | All + approvers |
| F-18 | Permission management | M | Delivered | All + approvers |
| F-19 | Work-from-home management | M | Delivered | All + approvers |
| F-20 | Notification centre (real-time + persistent) | M | Delivered | All |
| F-21 | Session audit and force-logout | M | Not delivered | Owner |
| F-22 | Sign-in | M | Partial | All |
| F-23 | Forward capacity forecasting | S | Not delivered | Owner, PM |
| F-24 | Automatic ageing and escalation | S | Not delivered | Team Leader, PM |
| F-25 | Estimate recalibration | C | Not delivered | PM |
| F-26 | User Tasks (per-person workload) | M | Delivered | PM, Section Head, Team Leader, Owner |
| F-27 | HR calendar | M | Delivered | All except Member (detail + approval) |

### 2.2 Feature descriptions

**F-01 Curriculum catalogue.** Five organisational levels: Academic Year → Curriculum Project → Term → Subject Group → Subject. A level cannot be deleted while it still contains descendants. Any level can be archived. Subjects carry lifecycle status: Active, On Hold, Closed, Reopened.

**F-02 Content decomposition.** A Subject breaks into Units, Lessons, and Learning Objectives. Creating a learning objective requires choosing a workflow schema and records production metadata (tag, environment, template, timestamps). Archiving a Unit or Lesson cascades to descendant learning objectives, tasks, and comments.

**F-03 Workflow schema designer.** Named, reusable production processes. Stages form a network (multiple predecessors and successors), not only a straight line. Each stage contains ordered steps with duration and priority. Schemas can be duplicated, archived, and unarchived. Removing a stage or step first reports how much live work would be affected, then migrates or advances surviving tasks.

**F-04 Task Bank.** Catalogue of work types: name, responsible discipline (Group), default duration, Creation vs Review, Team-Leader flag, active flag. This is how the engine knows which discipline owns generated work.

**F-05 Automated task generation.** On learning-objective creation, tasks are generated for every step of the start stage. Team-Leader work starts in To Do queued to the leader; other work starts in the discipline Backlog. Completing a task generates the next step in the same stage, or — if the stage is finished — every following stage whose prerequisites are fully complete. When no incomplete task remains in a Subject, the Subject closes and the Owner is notified. Standalone (ad-hoc) tasks are allowed as an escape hatch.

**F-06 Task execution.** Lifecycle Backlog → To Do → Doing → Done, plus Rollback. Users claim, proceed, pause/resume, flag, comment (threaded), and change priority. Effort is recorded as timed sessions with an explicit end reason. Boards and sheets are interchangeable; the preference is remembered. After 10 minutes of inactivity, active tasks are paused so effort stays honest.

**F-07 Assignment and workload.** Managers assign within the task’s discipline (Owner and PM may cross disciplines). Members cannot assign. Assignment notifies the assignee. Reassignment closes any open effort session. Managers see per-user in-progress and queued counts.

**F-08 Rework.** Only from a task that is in progress, and only to steps configured as rollback targets. Requires clarification, at least one originating step, and a **problem type** from the closed list: Content, Logic, UI, API, Performance, Development, VO/Narration, Others. Also captures per-step issue notes and up to 5 files of 10 MB each (PDF/Office). The new task is marked as rework, linked to its origin, and increments a rework counter. Full history is retrievable.

**F-09 Managerial intervention.** Skip a task; jump a workflow forward (Owner/PM); change the schema on a learning objective in flight, including auto-completing work up to defined stopping points (PM); complete a task on another user’s behalf (Owner/PM). All interventions are journaled as distinct event types.

**F-10 Sprints.** Named time-boxed increments with a date range. Learning objectives attach to sprints (an LO may appear in more than one). Sprint boards span projects and include work generated after the sprint opened. Sprints are archivable.

**F-11 Role-based dashboards.** Landing screen is determined by role. Owner and PM share an organisation-wide dashboard. Team Leader sees team-scoped metrics. Section Head and Member each have a dedicated dashboard UI and API. Business sign-off of widget content is still recommended.

**F-12 Daily operational report.** Filterable by team, semester, subject, grade, status, problem type, task name, and priority. Summary figures, charts (including counts by problem type), CSV/Excel export. Managers may attach one explanatory note per task.

**F-13 Task Logger.** Productivity report with no manual entry. Actual effort from work sessions (fallback: task duration). Expected effort from Step → Task Bank → task. Points: 3 early, 2 on time, 1 late, 0 if no estimate. Top 5 members for All Time, This Week, This Month. Excludes archived content, unassigned work, and archived users. Data is scoped to the viewer’s responsibility.

**F-14 Project and sprint analytics.** Learning-objective progress and status distribution; outstanding work by discipline using each Group’s colour; LO Progress tab listing work still in Backlog / To Do / Doing. Periods: Today, Last Week, Last Month, All Time. Several analytics SQL paths still exclude learning objectives whose name contains “old” (known gap).

**F-15 Hierarchical / SSRS reporting.** Date-ranged project reports, per-project breakdowns, and an embedded SSRS report. Screens exist but are not reachable from navigation.

**F-16 Organisation administration.** Groups (name + colour), Teams, Sections (head + groups), Users (identity, role, discipline, team, team leader, entitlements, internal/external). User removal archives. Every user-record change is journaled.

**F-17 Leave.** Types: Annual, Sick, Emergency, Unpaid, Borrowed-from-next-balance. Working days Monday–Friday. Preview before submit. Sick leave over 3 days requires a medical certificate. Emergency leave blocked in a configured blackout window. Borrowing capped (default 3 days) inside a configured window. Approval chain frozen at submission. Owner is final approver and triggers email. Pending requests are cancellable. Annual entitlements reset on the configured date, once per year, journaled.

**F-18 Permission.** Timed absence on a single date: work assignment, early departure, late arrival, departure. Available slots = ceiling minus used and pending. Owner requests auto-approve and email the CEO. Counters reset on the 21st of each month.

**F-19 Work-from-home.** Single-date requests. No duplicate active request for the same date; no past dates; capped at the user’s ceiling. Same approval chain as leave. Counters reset on the 21st of each month.

**F-20 Notifications.** Thirteen real-time event types. Every event also persisted. Categories (General, Leaves, Work Updates) and types (Project, Sprint, Leave, Task, System). Inbox: pagination, filters, unread count, mark read, deep links, inline approve/reject. Managers receive a live pending-decision count.

**F-21 Session audit.** **Not delivered.** There is no session table, Owner session browser, 24-hour storyline, or force-logout API. Presence is an in-memory SignalR map; disconnected users’ open tasks are paused after 30 minutes.

**F-22 Sign-in.** Unique 6-character code. JWT access token lasts 2 days; refresh token cookie lasts 10 days. Logout and refresh endpoints exist. After SignalR disconnect, open tasks are paused at 30 minutes. **Gaps:** no rate limiting/lockout; JWT issuer/audience not validated; access token stored in browser storage.

**F-23–F-25 (not delivered).** Forward capacity forecast; automatic ageing alerts; feeding observed averages back into default durations.

**F-26 User Tasks.** Managers (not Members) see a table of users with To Do and Doing counts and can open that user’s tasks (`/user-tasks`).

**F-27 HR calendar.** Non-members use `/calendar` (leave, permission, WFH tabs) plus detail pages to review and decide requests. Members use My Leaves only.

---

## 3. User requirements

User requirements are stated as capabilities a person must have. They map to features in Section 2 and to acceptance criteria in Section 6.

### 3.1 Production Member

| ID | User requirement |
|---|---|
| UR-M-01 | I need to see only the work that belongs to my discipline (and my assigned tasks), so I know what I can claim next. |
| UR-M-02 | I need to claim a task from the group backlog, start it, pause it when interrupted, and complete it. |
| UR-M-03 | I need comments (including replies) so I can discuss the work on the task itself. |
| UR-M-04 | I need to flag a blocked task so my Team Leader is notified immediately. |
| UR-M-05 | I need to send work back to a permitted earlier step, with a problem type, notes, and files, when the work is not acceptable. |
| UR-M-06 | I need Kanban and spreadsheet views of the same work, and I need the system to remember which I prefer. |
| UR-M-07 | I need to request leave, permission, and work-from-home, preview the balance impact, and cancel while pending. |
| UR-M-08 | I need a notification inbox that still has my alerts if I was offline. |
| UR-M-09 | I need a personal landing view of my own work and leave after I sign in. |

### 3.2 Team Leader

| ID | User requirement |
|---|---|
| UR-TL-01 | I need a team-scoped dashboard of members, projects, and task distribution. |
| UR-TL-02 | I need tasks marked as Team-Leader work to arrive in my To Do, not in the group backlog. |
| UR-TL-03 | I need to be notified when a member flags a task, and to see my team’s workload so I can reassign. |
| UR-TL-04 | I need to assign work only within my discipline. |
| UR-TL-05 | I need to record the first opinion on leave / permission / WFH for my direct reports. |
| UR-TL-06 | I need sprint boards and sprint analytics for the increment my team is delivering. |
| UR-TL-07 | I need Task Logger and the daily report scoped to my group. |
| UR-TL-08 | I need a per-person view of To Do and Doing counts so I can open a member’s work. |
| UR-TL-09 | I need a calendar of pending leave, permission, and WFH for my reports. |

### 3.3 Section Head

| ID | User requirement |
|---|---|
| UR-SH-01 | I need a section-scoped view of tasks and workload across the groups I oversee. |
| UR-SH-02 | I need to approve HR requests for users in my section’s groups. |
| UR-SH-03 | I need reporting scoped to my section, not the whole organisation. |
| UR-SH-04 | I need a live count of requests waiting for my decision. |
| UR-SH-05 | I need User Tasks and calendar views scoped to my section. |

### 3.4 Project Manager / Coordinator

| ID | User requirement |
|---|---|
| UR-PM-01 | I need to build and maintain the five-level curriculum catalogue and decompose subjects into units, lessons, and learning objectives. |
| UR-PM-02 | I need to design reusable workflows (stages, steps, Task Bank, rollback paths) and duplicate them for variants. |
| UR-PM-03 | I need every new learning objective to be bound to a schema so work is generated without my routing it. |
| UR-PM-04 | I need to staff subjects with people and be able to override the pipeline (skip, jump, change process) when reality diverges from the plan. |
| UR-PM-05 | I need to create sprints, attach learning objectives, and see increment health. |
| UR-PM-06 | I need organisation-wide dashboards and analytics (progress, discipline workload, daily report, Task Logger). |
| UR-PM-07 | I need to administer users, groups, teams, and sections, and see a journal of user-record changes. |
| UR-PM-08 | I need to know, before I remove a stage or step, how much live work would be affected. |
| UR-PM-09 | I need to reconstruct which process a completed item actually followed after a schema change. **Not met** (no schema versioning). |
| UR-PM-10 | I need a forward view of capacity vs confirmed absence. **Not met.** |

### 3.5 Owner

| ID | User requirement |
|---|---|
| UR-O-01 | I need the same organisation-wide production visibility as the Project Manager. |
| UR-O-02 | I need to be the final approver on leave, and to be notified when a subject closes. |
| UR-O-03 | I need my own permission requests auto-approved, with the CEO emailed. |
| UR-O-04 | I need to see every staff member’s leave balances. |
| UR-O-05 | I need a complete record of who signed in, from where, for how long, and why they left — including a 24-hour storyline. **Not met** (no session store). |
| UR-O-06 | I need to force a user out immediately and have their browser signed out. **Not met.** |

### 3.6 Cross-cutting (all authenticated users)

| ID | User requirement |
|---|---|
| UR-X-01 | I need to sign in with my issued code and sign out when I am done. |
| UR-X-02 | I must not be signed out while I am actively working. Met at current 2-day JWT (the previous 10-minute token assumption does not match code). |
| UR-X-03 | I must only see navigation and data my role is entitled to. |
| UR-X-04 | I need operational events (assignment, flag, HR decision, project close) both as a live toast and as a durable inbox item. |

---

## 4. Functional requirements

The catalogue below is the product contract. IDs align with `docs/BRD.md` Section 7 so engineering, QA, and business share one numbering scheme.

### 4.1 Curriculum and content

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-001 | Maintain a five-level catalogue: Academic Year → Curriculum Project → Term → Subject Group → Subject. | M | Delivered |
| FR-002 | Prevent deletion of any catalogue level that still contains descendants. | M | Delivered |
| FR-003 | Allow any catalogue level, subject, unit, lesson, or learning objective to be archived instead of deleted. | M | Delivered |
| FR-004 | Allow a Subject to be decomposed into Units, Lessons, and Learning Objectives. | M | Delivered |
| FR-005 | Record Subject lifecycle: Active, On Hold, Closed, Reopened. | M | Delivered |
| FR-006 | Require a workflow schema when a Learning Objective is created. | M | Delivered |
| FR-007 | Record production metadata on each Learning Objective: tag, environment, template, created/started/completed timestamps. | M | Delivered |
| FR-008 | Archiving a Unit or Lesson shall cascade to descendant learning objectives, tasks, and comments. | M | Delivered |
| FR-009 | Restrict curriculum and content administration to Project Manager and Owner. | M | Partial — UI only; API not fully restricted |

### 4.2 Workflow definition

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-010 | Allow reusable workflow schemas, optionally classified by schema type. | M | Delivered |
| FR-011 | Allow stages as a network with multiple predecessors and successors. | M | Delivered |
| FR-012 | Allow ordered steps within each stage, each with expected duration and priority. | M | Delivered |
| FR-013 | Maintain a Task Bank bound to Group, default duration, Creation/Review, Team-Leader flag, active flag. | M | Delivered |
| FR-014 | Allow permitted rollback targets of each step to be configured explicitly. | M | Delivered |
| FR-015 | Allow stages and steps to be reordered. | M | Delivered |
| FR-016 | Allow a schema to be duplicated. | S | Delivered |
| FR-017 | Allow schemas to be archived and unarchived. | S | Delivered |
| FR-018 | Before a stage or step is removed, report the volume of live work that would be affected. | M | Delivered |
| FR-019 | On removal, migrate or advance surviving tasks rather than orphaning them. | M | Delivered |
| FR-020 | Preserve, for each completed item, the version of the process it actually followed. | S | Not delivered |

### 4.3 Automated work generation

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-021 | On Learning Objective creation, generate a task for every step of the schema’s starting stage. | M | Delivered |
| FR-022 | Team-Leader work types queue to the leader (To Do); others enter the discipline backlog unassigned. | M | Delivered |
| FR-023 | On completion, generate the task for the next step in the same stage. | M | Delivered |
| FR-024 | On completion of a stage’s final step, generate work for each following stage only where every prerequisite stage is fully complete. | M | Delivered |
| FR-025 | Generated tasks inherit the responsible discipline from the Task Bank entry. | M | Delivered |
| FR-026 | When no incomplete task remains in a Subject, set the Subject to Closed and notify the Owner. | M | Delivered |
| FR-027 | Allow a standalone task to be created outside the automated flow. | S | Delivered |

### 4.4 Task execution

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-028 | Support Backlog → To Do → Doing → Done, plus Rollback. | M | Delivered |
| FR-029 | A user can claim an unassigned task from their discipline’s backlog. | M | Delivered |
| FR-030 | Record effort as timed work sessions with an explicit end reason. | M | Delivered |
| FR-031 | Pause and resume a task, suspending effort recording accordingly. | M | Delivered |
| FR-032 | Flag a task, notifying the Team Leader immediately and persistently. | M | Delivered |
| FR-033 | Comment on tasks, including threaded replies; edit and delete own comments. | M | Delivered |
| FR-034 | Task priority shall be adjustable. | M | Delivered |
| FR-035 | Journal every task event to an actor and timestamp across at least: creation, each status change, pause, resume, flag, unflag, assign, comment, comment edit/delete, rollback, priority change, skip, process change, jump, reactivation. | M | Delivered (22 event types) |
| FR-036 | Present tasks as Kanban and spreadsheet; remember the user’s preference. | M | Delivered |
| FR-037 | Scope task lists to what the viewer’s role may see. | M | Delivered |
| FR-038 | Automatically pause a user’s active tasks after 30 minutes of SignalR disconnect (in-memory presence). | M | Delivered |
| FR-039 | Alert when a task exceeds expected duration or waits unclaimed beyond a threshold. | S | Not delivered |

### 4.5 Assignment

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-040 | A manager can assign a task to a specific user. | M | Delivered |
| FR-041 | Members shall not assign tasks. | M | Delivered |
| FR-042 | Assignment restricted to the task’s discipline, except Owner and Project Manager. | M | Delivered |
| FR-043 | Assignment notifies the assignee in real time and persistently, with a link to the task. | M | Delivered |
| FR-044 | Reassignment closes any open effort session with a reassignment reason. | M | Delivered |
| FR-045 | Present per-user workload (in-progress and queued counts) to managers, including the User Tasks screen (not Members). | M | Delivered |

### 4.6 Rework

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-046 | Rework only from a task that is in progress. | M | Delivered |
| FR-047 | Rework targets limited to configured rollback targets. | M | Delivered |
| FR-048 | Capture per-step issue notes, a free-text clarification, and a required problem type from: Content, Logic, UI, API, Performance, Development, VO/Narration, Others. | M | Delivered |
| FR-049 | Accept supporting files: up to 5 files of 10 MB, PDF or Office formats. | M | Delivered |
| FR-050 | Mark the resulting task as rework, link to origin, increment rework counter. | M | Delivered |
| FR-051 | Retrievable rework history; attachments downloadable. | M | Delivered |
| FR-052 | Restrict rework-attachment access to users entitled to see the underlying task. | M | Not delivered (download unauthenticated) |

### 4.7 Managerial intervention

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-053 | A manager can skip a task and advance the workflow. | M | Delivered |
| FR-054 | Owner and Project Manager can jump a workflow forward to a permitted later stage. | M | Delivered |
| FR-055 | Project Manager can change the schema on a Learning Objective in flight, including auto-completing work up to defined stopping points. | M | Delivered |
| FR-056 | Owner and Project Manager can complete a task on another user’s behalf. | M | Delivered |
| FR-057 | All interventions journaled as distinct event types. | M | Delivered |

### 4.8 Sprints

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-058 | Support time-boxed sprints with name, description, and date range. | M | Delivered |
| FR-059 | Learning Objectives assignable to sprints, including more than one sprint. | M | Delivered |
| FR-060 | Sprint boards and sheets span projects and include work generated after the sprint opened. | M | Delivered |
| FR-061 | Sprints archivable and unarchivable. | S | Delivered |
| FR-062 | Sprint creation restricted to Project Manager and Owner. | M | Partial — UI only |

### 4.9 Dashboards

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-063 | Landing screen determined by the signed-in user’s role. | M | Delivered |
| FR-064 | Owner and Project Manager see an organisation-wide dashboard (users, projects, schemas, active work, learning-objective status). | M | Delivered |
| FR-065 | Team Leader sees a team-scoped dashboard (members, projects, task distribution). | M | Delivered |
| FR-066 | Section Head sees a section-scoped dashboard. | M | Delivered |
| FR-067 | Member sees a personal dashboard (own work and leave). | M | Delivered |
| FR-068 | A dashboard request from the wrong role is refused. | M | Delivered |
| FR-069 | Navigation shown only where the user’s role permits the function. | M | Delivered |

### 4.10 Reporting

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-070 | Report learning-objective progress and status at project and sprint scope. | M | Delivered |
| FR-071 | Analytics filterable by Today, Last Week, Last Month, All Time. | M | Delivered |
| FR-072 | Report outstanding work across disciplines using each discipline’s colour. | M | Delivered |
| FR-073 | A learning objective is complete only when it has work and none is incomplete; not started when it has no work but work is expected, or all work is still in Backlog. | M | Delivered |
| FR-074 | Daily operational report filterable by team, semester, subject, grade, status, problem type, task name, and priority, with summaries and charts (including problem-type counts). | M | Delivered |
| FR-075 | A manager can attach one explanatory note per task in the daily report. | M | Delivered |
| FR-076 | Productivity report derived entirely from recorded data. | M | Delivered |
| FR-077 | Award 3 points for finishing ahead of expected duration, 2 on time, 1 late, 0 where no expectation exists. | M | Delivered |
| FR-078 | Rank top 5 members by points over All Time, This Week, and This Month independently. | M | Delivered |
| FR-079 | Exclude archived content, unassigned work, and work assigned to archived users. | M | Delivered |
| FR-080 | Scope report data to the viewer’s responsibility. | M | Delivered |
| FR-081 | Report data exportable to CSV and Excel. | M | Delivered |
| FR-082 | Hierarchical management reports by date range and per project, plus SSRS. | S | Partial — built but not in navigation |

### 4.11 Organisation administration

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-083 | Groups with name and colour used consistently in analytics. | M | Delivered |
| FR-084 | Teams with user assignment. | M | Delivered |
| FR-085 | Sections with a Head and a set of Groups. | M | Delivered |
| FR-086 | User records: identity, contact, role, discipline, team, team leader, onboarding, internal/external. | M | Delivered |
| FR-087 | User records carry leave, permission, and work-from-home ceilings and balances. | M | Delivered |
| FR-088 | Each user has a unique 6-character code used as the sign-in credential. | M | Delivered |
| FR-089 | User removal archives rather than deletes. | M | Delivered |
| FR-090 | Every user-record change journaled (actor, action, detail, time) and viewable on the profile. | M | Delivered |
| FR-091 | User administration restricted to Project Manager and Owner. | M | Not delivered at API (unauthenticated user API historically) |

### 4.12 Leave

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-092 | Five leave types: Annual, Sick, Emergency, Unpaid, Borrowed-from-next-balance. | M | Delivered |
| FR-093 | Duration on working days only, excluding Saturday and Sunday. | M | Delivered |
| FR-094 | Preview working-day count and resulting balance before submit. | M | Delivered |
| FR-095 | Sick leave of more than 3 days requires a medical certificate. | M | Delivered |
| FR-096 | Emergency leave blocked during the configured blackout window before annual reset. | M | Delivered |
| FR-097 | Borrowing against next year capped (default 3 days) and only inside the configured window. | M | Delivered |
| FR-098 | Validate against remaining balance, including pending requests. | M | Delivered |
| FR-099 | Capture Team Leader and Section Head at submission so later org changes do not alter the chain. | M | Delivered |
| FR-100 | Multi-level approval with Owner as final approver; one opinion per approver with optional comment. | M | Delivered |
| FR-101 | Owner final approval sends a confirmation email. | M | Delivered |
| FR-102 | Managers can approve or reject several requests in one action. | S | Delivered |
| FR-103 | A user can cancel their own pending request. | M | Delivered |
| FR-104 | Medical certificates retrievable only by entitled users. | M | Not delivered (public web root) |
| FR-105 | Owner can view leave balances of all staff. | M | Delivered |
| FR-106 | Annual entitlements reset automatically on the configured date, once per year, journaled. | M | Delivered |

### 4.13 Permission and work-from-home

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-107 | Four permission types on a single date with from-and-to time. | M | Delivered |
| FR-108 | Available permission slots = ceiling minus used and pending. | M | Delivered |
| FR-109 | Owner permission requests auto-approved; CEO emailed. | M | Delivered |
| FR-110 | Single-date work-from-home requests. | M | Delivered |
| FR-111 | Reject duplicate active WFH for the same date, and past dates. | M | Delivered |
| FR-112 | Validate WFH against the user’s ceiling. | M | Delivered |
| FR-113 | Permission and WFH counters reset for all users on the 21st of each month. | M | Delivered |
| FR-114 | Permission and WFH follow the same approval chain as leave. | M | Delivered |
| FR-115 | Permission and WFH cancellable while pending. | M | Delivered |

### 4.14 Notification

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-116 | Deliver operationally significant events to signed-in users in real time. | M | Delivered (13 event types) |
| FR-117 | Persist every real-time notification so it survives the recipient being offline. | M | Delivered |
| FR-118 | Classify by category and type. | M | Delivered |
| FR-119 | Inbox: pagination, category filter, 7/30/90-day filter, unread count, mark-one and mark-all read. | M | Delivered |
| FR-120 | Notifications link directly to the relevant record. | M | Delivered |
| FR-121 | Actionable notifications approvable or rejectable from the inbox. | M | Delivered |
| FR-122 | Managers receive a live count of requests awaiting their decision. | M | Delivered |
| FR-123 | Notifications deliverable by email as well as in-app. | S | Partial — Owner leave approval and Owner permission auto-approval only |

### 4.15 Authentication and session

| ID | Requirement | P | Status |
|---|---|---|---|
| FR-124 | Users sign in with their unique 6-character code. | M | Delivered |
| FR-125 | Every sign-in opens a session record (time, IP, device). | M | Not delivered |
| FR-126 | A new sign-in closes any previously open session, recorded as replaced. | M | Not delivered (refresh tokens exist; no session journal) |
| FR-127 | Every session records why it ended: manual, expiry, inactivity, administrative, or replacement. | M | Not delivered |
| FR-128 | Sessions left open beyond token expiry are closed automatically within one minute. | M | Not delivered |
| FR-129 | Thirty minutes after SignalR disconnect, open tasks are paused. | M | Delivered |
| FR-130 | An expired or archived user is prevented from continuing to navigate. | M | Partial — archived users rejected on about-me; no dedicated expired-user entity |
| FR-131 | Owner can browse all sessions, filtered by date range and reason. | M | Not delivered |
| FR-132 | Owner can view a user’s 24-hour activity storyline. | M | Not delivered |
| FR-133 | Owner can force a user out immediately (invalidate access; sign out browser in real time). | M | Not delivered |
| FR-134 | Every function restricted to entitled roles, enforced by the server. | M | Not delivered consistently |
| FR-135 | Sign-in protected against automated guessing (rate limiting and lockout). | M | Not delivered |
| FR-136 | An active user is not signed out while working. | M | Delivered at current 2-day access token |
| FR-137 | Approvers (not Members) can review leave, permission, and WFH from a calendar and request-detail pages. | M | Delivered |

---

## 5. Constraints

### 5.1 Business and operational constraints

| ID | Constraint | Implication |
|---|---|---|
| C-01 | Users are known to HR and issued a unique 6-character code. There is no password and no SSO. | Cannot satisfy a corporate authentication standard without a change. |
| C-02 | Access token lifetime is 2 days; refresh token 10 days. | Long-lived browser session unless the user logs out. |
| C-03 | Only one active session per user. | A user cannot work on two devices at once. |
| C-04 | Working week is Monday–Friday for all staff. | No shift or regional variation. |
| C-05 | Permission and work-from-home counters reset on the 21st (payroll month boundary). | Policy is compiled in; changing the day requires a release. |
| C-06 | Production artefacts live in each discipline’s own tools. | ATS tracks work, not deliverables (except rollback files and medical certificates). |
| C-07 | Internal network / reverse proxy (`ats.stp.local`). | Session IP depends on forwarded headers. |
| C-08 | English-only UI. | Constrains usability for Arabic-first staff. |
| C-09 | Web only; no native mobile app. | Approvals away from a desk depend on the existing web UI (or email, which is limited). |
| C-10 | Workflow schemas are not versioned. | Historical reporting cannot reconstruct the process a completed item followed. |
| C-11 | Sprints reference learning objectives, not tasks. | Sprint scope is not frozen; late-generated work is pulled in automatically. |
| C-12 | One person is assumed to work one task at a time. | Overlapping effort sessions are not modelled. |

### 5.2 Technical constraints

| ID | Constraint | Implication |
|---|---|---|
| C-13 | SQL Server is the system of record. Loss of the database is total loss of service. | |
| C-14 | SMTP is required for Owner leave confirmation and CEO permission mail. | In-app notification continues if mail is down. |
| C-15 | SSRS is required only for the advanced report screen. | All other reporting continues without it. |
| C-16 | Presence state is in-process memory; SignalR has no backplane. | The application tier cannot be scaled horizontally without breaking inactivity detection and force-logout. |
| C-17 | Most operational thresholds are compiled constants (session lifetimes, scoring scale, upload limits, working week, sick-certificate threshold). | Policy changes require a software release. |
| C-18 | Client API address is compiled into the SPA. | Production builds must not point at a developer machine. |
| C-19 | TypeScript build errors are currently ignored. | Type-checking is not a release gate. |
| C-20 | No Dockerfile / CI pipeline found. | Quality gates cannot be enforced automatically. |

### 5.3 Security and compliance constraints (current gaps — treat as blockers)

| ID | Constraint / finding | Product impact |
|---|---|---|
| C-21 | Server-side authorisation is inconsistent; many APIs have no role policy. | The role model the business relies on is not fully enforced. |
| C-22 | Login has no rate limiting or lockout. | The 6-character code space can be guessed. |
| C-23 | JWT issuer and audience are not validated. | Any token signed with the shared key is accepted. |
| C-24 | Signing key, encryption key, database and mail credentials live in source-controlled configuration. | Repository access implies impersonation risk. |
| C-25 | API definition (Swagger) is published in every environment. | Attack surface is discoverable. |
| C-26 | Rework attachments and medical certificates are stored under the publicly served folder. | Health documents may be retrievable without entitlement. |
| C-27 | Access token is stored in browser local storage. | An injected script can steal a live session. |

### 5.4 Out-of-scope constraints (will not be built in v1.0)

Payroll, clocking, content authoring, LMS delivery, financials, partner portal, native mobile, and full Arabic localisation are out of scope as stated in the Project Brief.

---

## 6. Acceptance criteria

Criteria are written so QA can pass or fail them without interpreting intent. **Must-pass for v1.0 production use** unless marked *Gap* (known not met) or *Sign-off* (built, awaiting business confirmation).

### AC-01 Curriculum catalogue

- Given a Project Manager, when they create Academic Year → Curriculum Project → Term → Subject Group → Subject, then each child is nested under its parent and appears in the catalogue tree.
- Given a catalogue level that still has descendants, when a user attempts to delete it, then the system refuses.
- Given any catalogue level, when it is archived, then it is hidden from active lists and retained for history.
- Given a Subject, when its status is set to Active, On Hold, Closed, or Reopened, then that status is persisted and used as the project lifecycle tab.

### AC-02 Learning objective and first-wave tasks

- Given a Lesson, when a Learning Objective is created without a schema, then creation is rejected.
- Given a valid schema with a start stage of N steps, when a Learning Objective is created, then N tasks are generated, each bound to that learning objective and to the Group from the Task Bank.
- Given a Task Bank entry marked Team-Leader, then its generated task starts in To Do (queued to the leader); otherwise it starts in Backlog unassigned.
- Given production metadata (tag, environment, template), when the Learning Objective is saved, then those fields are stored.

### AC-03 Workflow progression

- Given a completed task that is not the last step of its stage, when complete is confirmed, then the next step’s task is created and the current task is Done.
- Given the last step of a stage, and a successor stage with two prerequisite stages, when only one prerequisite is complete, then the successor’s tasks are **not** created.
- Given the last step of a stage, when **every** prerequisite of a successor is complete, then that successor’s tasks are created.
- Given no incomplete tasks remain anywhere in a Subject, then the Subject status becomes Closed and the Owner receives a project-closed notification (real-time and persisted).

### AC-04 Task execution

- Given an unassigned Backlog task in the user’s Group, when the user proceeds, then the task is claimed by that user and moves to To Do (or Doing per the proceed contract).
- Given a task in Doing, when the user starts work, then a work-time session opens; when they complete, pause, flag, or are reassigned, then the session closes with the matching end reason.
- Given a Doing task, when the user pauses and later resumes, then effort does not accrue during the pause.
- Given a flagged task, then the Team Leader receives a real-time alert and a persistent notification.
- Given comments, then replies thread; the author can edit and delete their comments; each action is journaled.
- Given board and sheet routes for the same project, then both show the same tasks and the last-used view is restored on return.
- Given 30 minutes of SignalR disconnect, then the user’s open tasks are paused with end reason Session. The browser is not force-signed-out; JWT remains valid until expiry or logout.

### AC-05 Assignment

- Given a Member, when they attempt to assign a task, then the operation is refused.
- Given a Team Leader, when they assign a task to a user in a different Group, then the operation is refused.
- Given Owner or Project Manager, when they assign across Groups, then the assignment succeeds.
- Given a successful assignment, then the assignee is notified in real time and in the inbox, with a link that opens the task.
- Given an in-progress work session, when the task is reassigned, then that session closes with reason Reassign.

### AC-06 Rework

- Given a task not in Doing, when rollback is attempted, then it is refused.
- Given a Doing task, when rollback targets are requested, then only configured earlier steps are returned.
- Given a valid rollback with problem type, notes, clarification, and ≤5 PDF/Office files ≤10 MB each (request ≤60 MB), then a new task is created at the target step, marked as rework, linked via origin id, with RollbackCount incremented.
- Given rollback without a recognised problem type, then it is refused.
- Given rollback history, then the chain and attachments are retrievable.
- *Gap:* Given an unauthenticated caller, when they request an attachment download, then they **must** be refused. Currently they are not — fail until fixed.

### AC-07 Managerial intervention

- Given skip, then the work timer closes with reason Skip, the workflow advances, and a Skip activity is recorded.
- Given Owner or Project Manager, when they jump to a permitted later stage, then a Jump activity is recorded and work is created at the target; a Member’s jump is refused.
- Given a Project Manager changing a Learning Objective’s schema, then the change is journaled as process change; defined stop steps can be auto-completed.
- Given Owner or Project Manager completing another user’s task, then complete succeeds; a Member cannot complete another user’s task.

### AC-08 Sprints

- Given name, description, and date range, when a Project Manager or Owner creates a sprint, then it appears in the sprint list.
- Given learning objectives attached to a sprint, when new tasks are later generated for those LOs, then those tasks appear on the sprint board and sheet.
- Given a sprint, when it is archived, then it is hidden from the active list and can be unarchived.
- *Sign-off:* sprint creation is restricted in the UI; server-side restriction must also pass (FR-062).

### AC-09 Dashboards and navigation

- Given each of the five roles, when that user signs in, then they land on the dashboard for their role and cannot retrieve another role’s dashboard API (401/403).
- Given Owner or Project Manager, then the landing view includes organisation-wide counts (users, projects, schemas, active tasks) and learning-objective status distribution.
- Given Team Leader, then the landing view is team-scoped (members, projects, task distribution).
- Given Section Head, then the landing view loads the section-head dashboard API and widgets.
- Given Member, then the landing view loads personal work (and leave) widgets.
- Given sidebar rules, then Members do not see User Management, Workflow, Projects, or Members Leaves; only Owner sees Members Leaves. There is no Sessions screen.

### AC-10 Daily report and Task Logger

- Given the daily report, when filters (team, semester, subject, grade, status, problem type, task name, priority) are applied, then the table, summaries, and charts match the filters.
- Given a manager note on a task, then at most one note exists per task and it appears on the report.
- Given CSV/Excel export, then the downloaded file contains the filtered rows.
- Given Task Logger for a period, then actual minutes come from work sessions of non-archived users (fallback: task duration); expected minutes resolve Step → Task Bank → task → 0.
- Given actual < expected, then 3 points; equal, 2; greater, 1; no expected, 0.
- Given rankings, then top 5 members are shown independently for All Time, This Week, and This Month.
- Given archived ancestors, unassigned tasks, or archived assignees, then those rows are excluded.
- Given Owner/PM, Section Head, Team Leader, Member, then each sees only their scoped data.

### AC-11 Project and sprint analytics

- Given time period Today / Last Week / Last Month / All Time, then charts and tables use that window.
- Given a learning objective with tasks all Done, then it counts as complete; with no tasks but expected steps, or all Backlog, then not started; otherwise in process.
- Given outstanding work, then it is grouped by Group name and colour.
- *Gap:* learning objectives whose name contains “old” must **not** be silently excluded; exclusion must use an explicit archive/deprecation flag. Currently name-substring exclusion fails this criterion.

### AC-12 Organisation administration

- Given Group colour, then the same colour appears on workload charts.
- Given user create/edit, then Code is unique and max 6 characters; entitlements are stored; changes appear on the user change log.
- Given user delete, then the record is archived, not physically removed.
- *Gap:* unauthenticated or non-PM/Owner callers must not create, edit, or archive users.

### AC-13 Leave, permission, work-from-home

- Given a date range spanning a weekend, then Saturday and Sunday are not counted as leave days.
- Given preview, then working-day count and resulting balance are shown before commit.
- Given sick leave > 3 days without a certificate, then submit is refused; with a certificate, it is stored.
- Given emergency leave inside the blackout window, then submit is refused.
- Given from-next-balance above the cap or outside the window, then submit is refused.
- Given a request exceeding remaining balance including pending, then submit is refused.
- Given submit, then Team Leader and Section Head ids are stored on the request and later org changes do not rewrite them.
- Given each approver, then only one opinion per request is accepted; Owner approval is final and sends email.
- Given bulk approve, then all selected pending requests receive the decision.
- Given the requester, when the request is still pending, then cancel restores the pending balance.
- Given Owner, then all-staff balances are visible; other roles do not see that screen.
- Given annual reset date, then entitlements reset once per year and the run is journaled (no double reset).
- Given permission, then available slots = MAX − (used + pending); Owner submit auto-approves and emails the CEO.
- Given WFH, then duplicate active same-date and past dates are refused; ceiling is enforced.
- Given midnight on the 21st local time, then Permission and WorkFromHome used counters reset for all users.
- *Gap:* medical certificates must not be reachable from the public web root or an unauthenticated URL.

### AC-14 Notifications

- Given assignment, flag, HR opinion, project assigned, project closed, then a signed-in recipient gets a toast **and** a persisted inbox row.
- Given the recipient was offline, when they next open the inbox, then the notification is present.
- Given category and 7/30/90-day filters, then the list matches.
- Given an actionable HR notification, then approve/reject from the inbox updates the request.
- Given a manager with pending requests, then the sidebar badge matches the pending count and updates live.

### AC-15 Sessions

- Given login with a valid 6-character code, then a JWT is issued (2 days) and a refresh token cookie is set (10 days).
- Given logout, then the refresh cookie is cleared.
- Given SignalR disconnect for 30 minutes, then open tasks are paused.
- *Gap:* no persisted session row, no Owner session browser, no 24-hour storyline, no force-logout.
- *Gap:* repeated failed logins must be rate-limited and locked out.
- Given a 2-day access token, a user who remains on the site is not signed out solely by a 10-minute wall clock.

### AC-16 Authorisation (release blocker)

- Given any API endpoint that mutates users, schemas, curriculum, tasks, HR, or reports, when called without a valid token, then the response is 401.
- Given a Member token, when calling Owner-only or PM-only operations (user admin, schema delete, force-logout, jump, all-staff balances), then the response is 403.
- Given Swagger in production, then it is not publicly reachable (or is authenticated).

These authorisation criteria are **not currently met** across the API surface. They are still the acceptance bar for calling the product secure.

---

## 7. Traceability

| Feature | User requirements | Functional requirements | Acceptance |
|---|---|---|---|
| F-01 Catalogue | UR-PM-01 | FR-001–FR-005, FR-009 | AC-01 |
| F-02 Content / LO | UR-PM-01, UR-PM-03 | FR-004–FR-008 | AC-02 |
| F-03–F-04 Schema / Task Bank | UR-PM-02, UR-PM-08 | FR-010–FR-019 | AC-02, AC-07 |
| F-05 Automation | UR-PM-03, UR-TL-02 | FR-021–FR-027 | AC-02, AC-03 |
| F-06 Execution | UR-M-01–UR-M-06 | FR-028–FR-038 | AC-04 |
| F-07 Assignment | UR-TL-03–04, UR-TL-08, UR-PM-04 | FR-040–FR-045 | AC-05 |
| F-08 Rework | UR-M-05 | FR-046–FR-052 | AC-06 |
| F-09 Intervention | UR-PM-04 | FR-053–FR-057 | AC-07 |
| F-10 Sprints | UR-TL-06, UR-PM-05 | FR-058–FR-062 | AC-08 |
| F-11 Dashboards | UR-M-09, UR-TL-01, UR-SH-01, UR-O-01 | FR-063–FR-069 | AC-09 |
| F-12–F-15 Reporting | UR-PM-06, UR-TL-07, UR-SH-03 | FR-070–FR-082 | AC-10, AC-11 |
| F-16 Org admin | UR-PM-07 | FR-083–FR-091 | AC-12 |
| F-17–F-19 HR | UR-M-07, UR-TL-05, UR-TL-09, UR-SH-02, UR-O-02–04 | FR-092–FR-115, FR-137 | AC-13 |
| F-20 Notifications | UR-X-04, UR-SH-04 | FR-116–FR-123 | AC-14 |
| F-21–F-22 Sessions | UR-O-05–06, UR-X-01–02 | FR-124–FR-136 | AC-15, AC-16 |
| F-23–F-25 Future | UR-PM-09–10 | FR-020, FR-039 | Not in v1.0 AC |
| F-26 User Tasks | UR-TL-08, UR-SH-05 | FR-045 | AC-05 |
| F-27 HR calendar | UR-TL-09, UR-SH-05 | FR-137 | AC-13 |

### Counts

| Measure | Count |
|---|---|
| Features | 27 (21 delivered, 2 partial, 4 not delivered) |
| User requirements | 43 |
| Functional requirements | 137 |
| Acceptance suites | 16 |

**v1.0 product intent is delivered** for pipeline, execution, sprints, HR, notifications, and reporting. Remaining must-fix items before treating the PRD as fully satisfied: FR-009/062/091/134 (server authorisation), FR-052/104 (document access), FR-125–133 (session audit), FR-135 (login lockout), FR-082 (report navigation), AC-11 name-substring exclusion, plus the three strategic features F-23–F-25.

---

*Prepared from the implemented solution. Maintain alongside `docs/PROJECT_BRIEF.md` and `docs/BRD.md`. Re-verify against source at each release.*
