# Business Requirements Document (BRD)
## Automated Task System (ATS) — Curriculum Production Management Platform

| Field | Value |
|---|---|
| **Document title** | Business Requirements Document — Automated Task System |
| **System** | Automated Task System (ATS) / Task Management System v1.0 |
| **Organisation** | Selah El-Telmeez — Educational Content Production |
| **Document version** | 1.0 |
| **Date** | 2026-08-11 |
| **Prepared from** | Reverse-engineered from the implemented solution, validated against source code |
| **Companion document** | `docs/SYSTEM_DESCRIPTION.md` (technical system description) |
| **Status** | For review |

### Document control

| Version | Date | Author | Change summary |
|---|---|---|---|
| 1.0 | 2026-08-11 | — | Initial BRD reconstructed from the delivered system |

### Approvals required

| Role | Name | Signature | Date |
|---|---|---|---|
| Business Owner | | | |
| Head of Content Production | | | |
| HR Manager | | | |
| IT / Engineering Lead | | | |
| Information Security | | | |

### A note on the nature of this document

This BRD is written **retrospectively**. The Automated Task System is already built and operating; this document captures the business requirements the system satisfies, states them in verifiable form so they can be signed off, and separates them from the requirements that are **not yet met** (Section 12). It therefore serves three purposes: a baseline for change control, an onboarding reference for business stakeholders, and a prioritised backlog for the next phase of investment.

---

## Table of contents

1. [Executive summary](#1-executive-summary)
2. [Business context](#2-business-context)
3. [Business objectives and success measures](#3-business-objectives-and-success-measures)
4. [Stakeholders](#4-stakeholders)
5. [Scope](#5-scope)
6. [Business processes](#6-business-processes)
7. [Functional requirements](#7-functional-requirements)
8. [Business rules](#8-business-rules)
9. [Non-functional requirements](#9-non-functional-requirements)
10. [Data requirements and reporting](#10-data-requirements-and-reporting)
11. [Assumptions, dependencies and constraints](#11-assumptions-dependencies-and-constraints)
12. [Gap analysis and change requests](#12-gap-analysis-and-change-requests)
13. [Risks](#13-risks)
14. [Implementation roadmap](#14-implementation-roadmap)
15. [Glossary](#15-glossary)
16. [Appendix — requirements traceability](#16-appendix--requirements-traceability)

---

## 1. Executive summary

### 1.1 Business problem

Selah El-Telmeez produces educational curriculum content through a multi-discipline pipeline. Every learning objective passes through a sequence of specialist stages — instructional design, subject-matter review, graphic design, voice-over, animation, development, quality assurance, translation and proofreading — each performed by a different team.

Before ATS, this pipeline was coordinated informally. The organisation could not reliably answer basic operational questions: which discipline currently owes work on which lesson, how much effort a content item has consumed against its estimate, why a piece of work was sent back, or whether a team has the capacity to absorb more work next month. Handoffs depended on individuals remembering to notify the next team. Rework had no audit trail. Workforce availability (leave, permissions, work-from-home) was tracked in a separate system, so production planning was blind to who would actually be at work.

### 1.2 Business solution

ATS replaces informal coordination with an **automated production pipeline**. Management defines each production process once as a reusable **workflow schema** — a graph of stages, each containing ordered steps, each step bound to the discipline that performs it and to an expected duration. Attaching a schema to a learning objective causes the system to generate the required work automatically and to keep generating downstream work as upstream work completes, respecting prerequisites. No human routes work between teams.

Around this engine, ATS delivers execution tools (Kanban boards, spreadsheet views, sprints, rollback with evidence), measurement (effort capture, productivity scoring, progress analytics), role-appropriate visibility for five organisational levels, and a complete workforce-administration module so that availability and production live in one system.

### 1.3 Business value delivered

| Value | Mechanism |
|---|---|
| **Elimination of manual work routing** | The workflow engine generates and assigns work to the responsible discipline automatically as prerequisites are satisfied. |
| **Process consistency and reuse** | Production processes are defined once as schemas and reused across every subject, and can be duplicated to create controlled variants. |
| **Complete auditability** | Every task transition is journaled across 22 event types; every rework carries issue notes, a clarification and supporting files. |
| **Effort visibility** | Actual work time is captured per user per session with an explicit end reason, and compared against expected duration. |
| **Right-sized visibility** | Five roles each see only the data and actions relevant to their responsibility. |
| **Unified availability and production data** | Leave, permission and work-from-home requests share the same users and organisational hierarchy as production work. |
| **Operational accountability** | Login sessions are persisted with reason codes, IP and device, with an administrative session browser and force-logout. |

### 1.4 Current state

The system is **in production use**. It comprises 27 API modules exposing approximately 200 endpoints, 50 user-facing screens, 38 persisted entities, and 5 automated background processes. The functional requirements in Section 7 are all implemented. Section 12 records 39 identified gaps, of which **7 are security issues rated critical** and require action before further business investment in new features.

---

## 2. Business context

### 2.1 Organisational structure as modelled

ATS encodes the production organisation in four structures that all resolve to a person:

| Structure | Business meaning |
|---|---|
| **Group** | A production discipline (Instructional Design, Graphic Design, Voice-Over, Animation, QA, Translation, and so on). Groups own work: a task belongs to a group before it belongs to a person. Each group has a colour that carries through to all analytics. |
| **Team** | A reporting unit of people, used for administrative grouping and reporting. |
| **Section** | An oversight layer spanning several Groups, headed by a Section Head. Determines whose leave a Section Head approves. |
| **Team Leader relationship** | A direct-report link on each user, which determines the first level of approval for their requests. |

### 2.2 Content structure as modelled

Content is catalogued in five organisational levels and then decomposed in three production levels:

```
Academic Year  →  Curriculum Project  →  Term  →  Subject Group  →  Subject
                                                                      │
                                            Unit  →  Lesson  →  Learning Objective
                                                                      │
                                                              Tasks (generated)
```

**Business significance of the split:** the upper five levels are catalogue structure and are protected against deletion while they still contain content. The lower levels are production content and cascade on removal. The **Learning Objective is the unit of production** — it is the level at which a workflow is attached, at which sprints are planned, and against which all effort and progress are measured. A **Subject** is the level at which the business declares work complete: when no incomplete tasks remain anywhere in a subject, the subject closes automatically.

### 2.3 The workflow concept in business terms

| Technical term | Business meaning |
|---|---|
| **Schema** | A named production process — for example "Standard Animated Lesson" or "Translated Assessment Item". Reusable across any number of subjects. |
| **Node** | A stage of production. Stages form a network, not a straight line: a stage may require several earlier stages to finish before it can start, and finishing one stage may release several stages in parallel. |
| **Step** | A discrete piece of work within a stage, with its own expected duration and priority. |
| **Task Bank entry** | A catalogued type of work — its name, the discipline that performs it, its default duration, whether it is creation or review work, and whether it is performed by a Team Leader. |
| **Task** | An actual instance of work, generated by the system, for one learning objective at one step. |
| **Rollback target** | A pre-approved earlier step that work may be sent back to. Rework paths are designed, not improvised. |

---

## 3. Business objectives and success measures

| ID | Business objective | Success measure | Current support |
|---|---|---|---|
| BO-01 | Eliminate manual coordination of handoffs between production disciplines | 100% of pipeline work generated by the system rather than created by hand | ✅ Delivered — automatic generation with prerequisite checking |
| BO-02 | Enforce a consistent, repeatable production process | Every learning objective is bound to a defined schema at creation | ✅ Delivered — schema selection is mandatory when creating a learning objective |
| BO-03 | Provide complete traceability of production and rework | Every status change and every rework event is attributable to a person and a time | ✅ Delivered — 22 activity types plus rollback records with evidence |
| BO-04 | Measure actual effort against estimates | Effort recorded for every task with an explainable start/stop reason | ✅ Delivered — work-time capture with 7 end reasons; Task Logger scoring |
| BO-05 | Give each management level the information relevant to its responsibility | Each of the 5 roles receives a role-appropriate landing dashboard and scoped data | ⚠️ Partially delivered — endpoints exist for all 5 roles; Section Head and Member dashboard content requires business sign-off (CR-26) |
| BO-06 | Deliver work in predictable time-boxed increments | Learning objectives assignable to sprints with sprint-level progress reporting | ✅ Delivered |
| BO-07 | Administer workforce availability inside the production system | Leave, permission and work-from-home requests processed end to end with balance enforcement | ✅ Delivered |
| BO-08 | Ensure managers act on requests and escalations promptly | Real-time notification plus persistent inbox with inline approval | ✅ Delivered — 13 real-time event types plus a durable notification centre |
| BO-09 | Maintain accountability for system access | Every session recorded with start, end, reason, IP and device; administrative force-logout available | ✅ Delivered — Owner-only session browser with 24-hour activity storyline |
| BO-10 | Forecast production capacity against confirmed availability | Forward capacity view combining estimates, historical throughput and approved absence | ❌ Not delivered — see CR-41 |
| BO-11 | Detect stalled work before it becomes a delivery risk | Automatic ageing alerts on work exceeding expected duration or dwell time | ❌ Not delivered — escalation is currently manual via flagging (CR-42) |
| BO-12 | Improve estimation accuracy over time | Observed durations feed back into default estimates | ❌ Not delivered — see CR-44 |

---

## 4. Stakeholders

### 4.1 System users

| Stakeholder | Role in system | Primary interest | Key capabilities |
|---|---|---|---|
| **Business Owner** | `Owner` | Overall delivery, workforce cost and accountability | Full visibility; final HR approver; only role that can view all member leave balances, browse sessions and force logout |
| **Project Manager / Coordinator** | `ProjectManger` | Getting content produced on process and on time | Designs the curriculum catalogue and workflow schemas; staffs projects; holds override powers (jump, skip, change process) |
| **Section Head** | `SectionHead` | Throughput across several disciplines | Section-scoped task and workload metrics; approves HR requests for users in their section's groups |
| **Team Leader** | `TeamLeader` | One discipline's output and team wellbeing | Team dashboard and workload view; receives flagged-task escalations; first-level HR approver for direct reports; performs Team-Leader-designated steps |
| **Production Member** | `Member` | Clarity on what to do next | Claims and works tasks; comments; flags blockers; requests rework; manages own leave |
| **HR Administrator** | via `Owner` / `ProjectManger` | Accurate entitlements and compliant approvals | Maintains user entitlement ceilings; reviews approval chains; audits changes via the user change log |

### 4.2 Non-user stakeholders

| Stakeholder | Interest |
|---|---|
| **CEO** | Receives email notification of Owner-approved permission requests |
| **Information Security** | Owns the critical findings in Section 12.1 |
| **IT Operations** | Deployment, database, SMTP and SSRS availability |
| **External contributors** | Represented by `AccountType = External`, but with no differentiated experience today (CR-45) |

---

## 5. Scope

### 5.1 In scope

| # | Capability area |
|---|---|
| 1 | Curriculum catalogue management (five organisational levels) |
| 2 | Content decomposition (Unit → Lesson → Learning Objective) |
| 3 | Workflow schema design, versionless editing, duplication and archival |
| 4 | Task Bank (catalogue of work types by discipline) |
| 5 | Automated task generation and prerequisite-driven progression |
| 6 | Task execution: claim, progress, pause, comment, prioritise, flag |
| 7 | Rework via configured rollback paths with issue notes and file evidence |
| 8 | Managerial workflow intervention: skip, jump, change process |
| 9 | Sprint planning and sprint-scoped execution |
| 10 | Role-based dashboards and workload visibility |
| 11 | Daily operational reporting with manager annotations |
| 12 | Productivity measurement and member ranking |
| 13 | Project and sprint progress analytics |
| 14 | Hierarchical management reporting and SSRS integration |
| 15 | Organisation administration: users, groups, teams, sections |
| 16 | Leave management (5 leave types) with multi-level approval |
| 17 | Short-duration permission management (4 types) |
| 18 | Work-from-home request management |
| 19 | Real-time and persistent notification with inline approval |
| 20 | Session auditing and administrative access control |
| 21 | Automated periodic processes (entitlement resets, session hygiene) |

### 5.2 Out of scope

| Excluded | Note |
|---|---|
| Payroll processing and time-and-attendance | ATS records absence approvals only; it is not a payroll or clocking system |
| Content authoring and asset storage | ATS tracks that work happened; the artefacts live in the teams' production tools |
| Learning delivery to students | ATS is a production system, not an LMS |
| Financial management, budgeting, procurement | No cost or budget concepts exist in the model |
| Customer or partner-facing portal | Internal system only |
| Formal password or single-sign-on authentication | Currently a 6-character code; see CR-02 |
| Native mobile applications | Web only; see CR-46 |
| Multi-language user interface | English only despite Arabic content; see CR-29 |

### 5.3 Scope assumptions

- Users are internal staff or contracted contributors known to HR and issued a system code.
- The system operates inside the corporate network or behind a corporate reverse proxy at `ats.stp.local`.
- Content artefacts remain in the production tools of each discipline; ATS references work, not files (with the exception of rollback evidence and medical certificates).

---

## 6. Business processes

### 6.1 BP-01 — Content production pipeline (primary value stream)

| Step | Actor | Action | System behaviour |
|---|---|---|---|
| 1 | Project Manager | Creates the catalogue path down to a Subject | Blocks deletion of any level that still contains content |
| 2 | Project Manager | Decomposes the Subject into Units, Lessons, Learning Objectives | Requires a workflow schema for every Learning Objective |
| 3 | **System** | Generates the first stage's work | Creates one task per step of the start stage; Team-Leader steps are queued directly to the leader, all others enter the discipline's backlog |
| 4 | Member | Claims a task from the discipline backlog | Assigns the task to the claimant |
| 5 | Member | Starts work | Opens an effort timer |
| 6 | Member | Pauses / resumes as interrupted | Closes and reopens the timer, preserving honest effort data |
| 7 | Member | Completes the task | Closes the timer as *complete*; journals the completion |
| 8 | **System** | Releases the next work | Next step in the same stage; or, if the stage is finished, every following stage whose prerequisites are all satisfied |
| 9 | Reviewer | Rejects the work and requests rework | Creates a task at a pre-approved earlier step with issue notes, clarification and evidence files; increments the rework counter |
| 10 | Member | Flags a blocker | Real-time alert plus persistent notification to the Team Leader; effort timer closed as *flagged* |
| 11 | Project Manager | Intervenes when necessary | Skip a task, jump ahead in the process, or change the process on a learning objective — all journaled |
| 12 | **System** | Closes the Subject | When no incomplete tasks remain in the Subject, sets it to *Closed* and notifies the Owner |

### 6.2 BP-02 — Workflow process design

| Step | Actor | Action |
|---|---|---|
| 1 | Project Manager | Maintains the Task Bank: work type, responsible discipline, default duration, creation-vs-review, Team-Leader flag |
| 2 | Project Manager | Creates a Schema, optionally by duplicating an existing one |
| 3 | Project Manager | Adds Stages and defines their prerequisite relationships |
| 4 | Project Manager | Adds ordered Steps to each Stage, each bound to a Task Bank entry |
| 5 | Project Manager | Configures which earlier Steps each Step may be rolled back to |
| 6 | Project Manager | Removes a Stage or Step | System first reports how much live work would be affected, then migrates or advances the surviving tasks |
| 7 | Project Manager | Archives a Schema when retired; may unarchive |

### 6.3 BP-03 — Sprint delivery

| Step | Actor | Action |
|---|---|---|
| 1 | Project Manager | Creates a sprint with a name, description and date range |
| 2 | Project Manager / Team Leader | Attaches Learning Objectives to the sprint |
| 3 | Team | Works the sprint board or sheet, which spans projects |
| 4 | Manager | Monitors sprint analytics: task health, completion state, discipline workload distribution |
| 5 | Project Manager | Archives the sprint when the increment closes |

Because a sprint references learning objectives rather than tasks, work generated after the sprint opens is automatically included.

### 6.4 BP-04 — Leave request and approval

| Step | Actor | Action | System behaviour |
|---|---|---|---|
| 1 | Employee | Previews a leave request | Returns the working-day count and the resulting balance before commitment |
| 2 | Employee | Submits the request | Validates working days (Mon–Fri), remaining balance, type-specific windows; requires a medical certificate for sick leave over 3 days |
| 3 | **System** | Routes for approval | Records the Team Leader and Section Head at submission time, freezing the chain; notifies approvers in real time and persistently |
| 4 | Team Leader | Records an opinion | One opinion per approver per request |
| 5 | Section Head / Project Manager | Records an opinion | Managers may clear a queue with a bulk decision |
| 6 | Owner | Records the final decision | Sends a confirmation email |
| 7 | **System** | Applies the outcome | Updates the balance; notifies the requester in real time |
| 8 | Employee | Cancels while still pending | Restores the pending balance |

### 6.5 BP-05 — Permission and work-from-home approval

The same approval chain applies with these business differences:

- **Permission** is a timed absence on a single date (work assignment, early departure, late arrival, departure). Available slots are the ceiling minus used and pending. **Owner requests are auto-approved** and the CEO is emailed.
- **Work from home** is a whole-day request. Duplicate active requests for the same date and past dates are rejected.
- Both counters **reset for all users on the 21st of each month**, aligning with the payroll month boundary.

### 6.6 BP-06 — Performance and productivity review

| Step | Actor | Action |
|---|---|---|
| 1 | Manager | Opens the Task Logger for a period, scoped automatically to their responsibility |
| 2 | **System** | Computes actual effort from recorded work sessions, falling back to the recorded task duration |
| 3 | **System** | Resolves expected effort from the Step, then the Task Bank entry, then the task |
| 4 | **System** | Awards 3 points for finishing early, 2 for on time, 1 for late, 0 where no estimate exists |
| 5 | **System** | Ranks the top 5 members over All Time, This Week and This Month |
| 6 | Manager | Reviews the daily report, annotates specific tasks, and exports for discussion |

### 6.7 BP-07 — Session governance

| Step | Actor | Action |
|---|---|---|
| 1 | User | Signs in with a 6-character code | Session opened with time, IP and device; any prior open session is closed as *replaced by new login* |
| 2 | **System** | Monitors presence | 10 minutes of inactivity pauses the user's tasks, invalidates their access and signs them out |
| 3 | **System** | Sweeps expired sessions every minute | Every session records why it ended |
| 4 | Owner | Reviews sessions and a user's 24-hour activity storyline | Filterable by date range and reason |
| 5 | Owner | Forces a user out | Access invalidated immediately and the user's browser is signed out in real time |

---

## 7. Functional requirements

Priority: **M** = Must have (implemented and essential), **S** = Should have, **C** = Could have.
Status: **✅** implemented, **⚠️** partially implemented, **❌** not implemented.

### 7.1 Curriculum and content management

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-001 | The system shall maintain a five-level curriculum catalogue: Academic Year → Curriculum Project → Term → Subject Group → Subject. | M | ✅ |
| FR-002 | The system shall prevent deletion of any catalogue level that still contains descendants. | M | ✅ |
| FR-003 | The system shall allow any catalogue level, subject, unit, lesson or learning objective to be archived instead of deleted. | M | ✅ |
| FR-004 | The system shall allow a Subject to be decomposed into Units, Lessons and Learning Objectives. | M | ✅ |
| FR-005 | The system shall record a lifecycle status on every Subject: Active, On Hold, Closed or Reopened. | M | ✅ |
| FR-006 | The system shall require a workflow schema to be selected when a Learning Objective is created. | M | ✅ |
| FR-007 | The system shall record production metadata on each Learning Objective: tag, environment, template, and created/started/completed timestamps. | M | ✅ |
| FR-008 | Archiving a Unit or Lesson shall cascade to its descendant learning objectives, tasks and comments. | M | ✅ |
| FR-009 | The system shall restrict curriculum and content administration to Project Manager and Owner. | M | ⚠️ Enforced in the user interface only; the API is not restricted (see CR-01) |

### 7.2 Workflow definition

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-010 | The system shall allow definition of reusable workflow schemas, optionally classified by schema type. | M | ✅ |
| FR-011 | The system shall allow schema stages to be arranged as a network with multiple predecessors and multiple successors, not only a linear sequence. | M | ✅ |
| FR-012 | The system shall allow ordered steps within each stage, each with its own expected duration and priority. | M | ✅ |
| FR-013 | The system shall maintain a Task Bank of work types, each bound to the responsible discipline (Group), a default duration, a creation-or-review classification, a Team-Leader flag and an active flag. | M | ✅ |
| FR-014 | The system shall allow the permitted rollback targets of each step to be configured explicitly. | M | ✅ |
| FR-015 | The system shall allow stages and steps to be reordered. | M | ✅ |
| FR-016 | The system shall allow a schema to be duplicated to create a variant. | S | ✅ |
| FR-017 | The system shall allow schemas to be archived and unarchived. | S | ✅ |
| FR-018 | Before a stage or step is removed, the system shall report the volume of live work that would be affected. | M | ✅ |
| FR-019 | On removal of a stage or step, the system shall migrate or advance surviving tasks rather than orphaning them. | M | ✅ |
| FR-020 | The system shall preserve, for each completed item, the version of the process it actually followed. | S | ❌ See CR-40 |

### 7.3 Automated work generation

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-021 | On creation of a Learning Objective, the system shall generate a task for every step of the schema's starting stage. | M | ✅ |
| FR-022 | Tasks derived from Team-Leader-designated work types shall be queued directly to the leader; all others shall enter the responsible discipline's backlog unassigned. | M | ✅ |
| FR-023 | On completion of a task, the system shall generate the task for the next step in the same stage. | M | ✅ |
| FR-024 | On completion of the final step of a stage, the system shall generate work for each following stage, but only where every one of that stage's prerequisites is fully complete. | M | ✅ |
| FR-025 | Every generated task shall inherit the responsible discipline from its Task Bank entry, requiring no manual routing. | M | ✅ |
| FR-026 | When no incomplete task remains within a Subject, the system shall set the Subject to Closed and notify the Owner. | M | ✅ |
| FR-027 | The system shall allow a standalone task to be created outside the automated flow. | S | ✅ |

### 7.4 Task execution

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-028 | The system shall support the task lifecycle Backlog → To Do → Doing → Done, plus a Rollback state. | M | ✅ |
| FR-029 | A user shall be able to claim an unassigned task from their discipline's backlog. | M | ✅ |
| FR-030 | The system shall record the effort spent on every task as timed work sessions with an explicit reason for each session ending. | M | ✅ |
| FR-031 | A user shall be able to pause and resume a task, suspending effort recording accordingly. | M | ✅ |
| FR-032 | A user shall be able to flag a task, notifying their Team Leader immediately and persistently. | M | ✅ |
| FR-033 | Users shall be able to comment on tasks, including threaded replies, and to edit and delete their comments. | M | ✅ |
| FR-034 | Task priority shall be adjustable. | M | ✅ |
| FR-035 | The system shall journal every task event, attributing it to an actor and a timestamp, across at least the following: creation, each status change, pause, resume, flag, unflag, assign, comment, comment edit, comment delete, rollback, priority change, skip, process change, jump, and reactivation. | M | ✅ 22 event types |
| FR-036 | Tasks shall be presentable both as a Kanban board and as a spreadsheet, with the user's preference remembered. | M | ✅ |
| FR-037 | Task lists shall be scoped to what the viewer's role permits them to see. | M | ✅ |
| FR-038 | The system shall automatically pause a user's active tasks after 10 minutes of inactivity so that recorded effort remains accurate. | M | ✅ |
| FR-039 | The system shall raise an alert when a task exceeds its expected duration or has waited unclaimed beyond a defined threshold. | S | ❌ See CR-42 |

### 7.5 Assignment and delegation

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-040 | A manager shall be able to assign a task to a specific user. | M | ✅ |
| FR-041 | Members shall not be permitted to assign tasks. | M | ✅ |
| FR-042 | Assignment shall be restricted to users of the task's own discipline, except for Owner and Project Manager who may cross discipline boundaries. | M | ✅ |
| FR-043 | Assignment shall notify the assignee in real time and persistently, with a direct link to the task. | M | ✅ |
| FR-044 | Reassignment shall close any open effort session with a reassignment reason. | M | ✅ |
| FR-045 | The system shall present per-user workload (in-progress and queued counts) to managers to support load balancing. | M | ✅ |

### 7.6 Rework (rollback)

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-046 | Rework shall only be initiable from a task that is in progress. | M | ✅ |
| FR-047 | Rework targets shall be limited to the steps configured as permitted rollback targets. | M | ✅ |
| FR-048 | A rework request shall capture per-step issue notes and a free-text clarification. | M | ✅ |
| FR-049 | A rework request shall accept supporting files up to 5 files of 10 MB each in PDF or Office formats. | M | ✅ |
| FR-050 | The resulting task shall be marked as rework, linked to its originating task, and shall increment a rework counter. | M | ✅ |
| FR-051 | The complete rework history of a task shall be retrievable, and its attachments downloadable. | M | ✅ |
| FR-052 | Access to rework attachments shall be restricted to users entitled to see the underlying task. | M | ❌ The download endpoint is unauthenticated (see CR-06) |

### 7.7 Managerial intervention

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-053 | A manager shall be able to skip a task and advance the workflow. | M | ✅ |
| FR-054 | Owner and Project Manager shall be able to jump a workflow forward to a permitted later stage. | M | ✅ |
| FR-055 | Project Manager shall be able to change the workflow schema applied to a Learning Objective in flight, including auto-completing work up to defined stopping points. | M | ✅ |
| FR-056 | Owner and Project Manager shall be able to complete a task on another user's behalf. | M | ✅ |
| FR-057 | All interventions shall be journaled as distinct event types. | M | ✅ |

### 7.8 Sprint management

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-058 | The system shall support time-boxed sprints with a name, description and date range. | M | ✅ |
| FR-059 | Learning Objectives shall be assignable to sprints, and to more than one sprint. | M | ✅ |
| FR-060 | Sprint boards and sheets shall span projects and shall include work generated after the sprint opened. | M | ✅ |
| FR-061 | Sprints shall be archivable and unarchivable. | S | ✅ |
| FR-062 | Sprint creation shall be restricted to Project Manager and Owner. | M | ⚠️ Enforced in the user interface only |

### 7.9 Dashboards and visibility

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-063 | The landing screen shall be determined by the signed-in user's role. | M | ✅ |
| FR-064 | Owner and Project Manager shall see an organisation-wide dashboard covering users, projects, schemas, active work and learning-objective status distribution. | M | ✅ |
| FR-065 | Team Leader shall see a team-scoped dashboard covering members, projects and task distribution. | M | ✅ |
| FR-066 | Section Head shall see a section-scoped dashboard. | M | ⚠️ Endpoint exists; content requires business sign-off (CR-26) |
| FR-067 | Member shall see a personal dashboard covering their own work and leave. | M | ⚠️ Endpoint exists; content requires business sign-off (CR-26) |
| FR-068 | A dashboard request from a user of the wrong role shall be refused. | M | ✅ |
| FR-069 | Navigation options shall be shown only where the user's role permits the underlying function. | M | ✅ |

### 7.10 Reporting and analytics

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-070 | The system shall report learning-objective progress and status distribution at both project and sprint scope. | M | ✅ |
| FR-071 | Analytics shall be filterable by period: Today, Last Week, Last Month, All Time. | M | ✅ |
| FR-072 | The system shall report the distribution of outstanding work across production disciplines, using each discipline's assigned colour. | M | ✅ |
| FR-073 | A learning objective shall be reported as complete only when it has work and none of it is incomplete; as not started when it has no work but work is expected, or when all its work is still in the backlog. | M | ✅ |
| FR-074 | The system shall provide a daily operational report filterable by team, semester, subject, grade, status and problem type, with summary figures and charts. | M | ✅ |
| FR-075 | A manager shall be able to attach an explanatory note to a specific task in the daily report, limited to one note per task. | M | ✅ |
| FR-076 | The system shall provide a productivity report derived entirely from recorded data, with no manual entry. | M | ✅ |
| FR-077 | The productivity report shall award 3 points for completing ahead of the expected duration, 2 for on time, 1 for late, and 0 where no expectation exists. | M | ✅ |
| FR-078 | The productivity report shall rank the top 5 members by points over All Time, This Week and This Month independently. | M | ✅ |
| FR-079 | The productivity report shall exclude work belonging to archived content, unassigned work, and work assigned to archived users. | M | ✅ |
| FR-080 | Report data shall be scoped to the viewer's responsibility: all data for Owner and Project Manager; section groups for Section Head; own group for Team Leader; own or group work for Member. | M | ✅ |
| FR-081 | Report data shall be exportable to CSV and Excel. | M | ✅ |
| FR-082 | The system shall provide hierarchical management reports by date range and per project, plus access to the corporate SSRS report. | S | ⚠️ Implemented but not reachable from navigation (CR-28) |

### 7.11 Organisation administration

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-083 | The system shall support creation and maintenance of production disciplines (Groups) with a name and a colour used consistently in analytics. | M | ✅ |
| FR-084 | The system shall support creation and maintenance of Teams with user assignment. | M | ✅ |
| FR-085 | The system shall support creation and maintenance of Sections, each with a Head and a set of Groups. | M | ✅ |
| FR-086 | The system shall support user records covering identity, contact details, role, discipline, team, team leader, onboarding state and internal-or-external classification. | M | ✅ |
| FR-087 | User records shall carry leave, permission and work-from-home entitlement ceilings and current balances. | M | ✅ |
| FR-088 | Each user shall have a unique 6-character code that serves as their sign-in credential. | M | ✅ |
| FR-089 | User removal shall archive the record rather than delete it. | M | ✅ |
| FR-090 | Every change to a user record shall be journaled with the actor, the action, the detail of the change and the timestamp, and shall be viewable on the user's profile. | M | ✅ |
| FR-091 | User administration shall be restricted to Project Manager and Owner. | M | ❌ The user API is unauthenticated (see CR-01) |

### 7.12 Leave management

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-092 | The system shall support five leave types: Annual, Sick, Emergency, Unpaid and Borrowed-from-next-balance. | M | ✅ |
| FR-093 | Leave duration shall be calculated on working days only, excluding Saturday and Sunday. | M | ✅ |
| FR-094 | A user shall be able to preview the working-day count and resulting balance before submitting. | M | ✅ |
| FR-095 | Sick leave of more than 3 days shall require a medical certificate upload. | M | ✅ |
| FR-096 | Emergency leave shall be blocked during the configured blackout window preceding the annual reset. | M | ✅ |
| FR-097 | Borrowing against next year's balance shall be capped at the configured maximum (default 3 days) and permitted only inside the configured window. | M | ✅ |
| FR-098 | Requests shall be validated against the user's remaining balance, including pending requests. | M | ✅ |
| FR-099 | The approval chain (Team Leader and Section Head) shall be captured at submission so later organisational changes do not alter it. | M | ✅ |
| FR-100 | Approval shall follow a multi-level chain with Owner as final approver, recording one opinion per approver with an optional comment. | M | ✅ |
| FR-101 | Final approval by the Owner shall send a confirmation email. | M | ✅ |
| FR-102 | Managers shall be able to approve or reject several requests in a single action. | S | ✅ |
| FR-103 | A user shall be able to cancel their own pending request. | M | ✅ |
| FR-104 | Medical certificates shall be retrievable only by users entitled to see them. | M | ❌ Stored in the public web root (see CR-06) |
| FR-105 | Owner shall be able to view the leave balances of all staff. | M | ✅ |
| FR-106 | Annual entitlements shall reset automatically on the configured date, once per year, with the execution journaled. | M | ✅ |

### 7.13 Permission and work-from-home management

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-107 | The system shall support four permission types: work assignment, early departure, late arrival and departure, each on a single date with a from-and-to time. | M | ✅ |
| FR-108 | Available permission slots shall be the user's ceiling less used and pending requests. | M | ✅ |
| FR-109 | Permission requests raised by the Owner shall be approved automatically and shall notify the CEO by email. | M | ✅ |
| FR-110 | The system shall support single-date work-from-home requests. | M | ✅ |
| FR-111 | Work-from-home requests shall be rejected where an active request already exists for the same date, or where the date is in the past. | M | ✅ |
| FR-112 | Work-from-home requests shall be validated against the user's ceiling. | M | ✅ |
| FR-113 | Permission and work-from-home counters shall reset for all users on the 21st of each month. | M | ✅ |
| FR-114 | Permission and work-from-home requests shall follow the same approval chain as leave. | M | ✅ |
| FR-115 | Permission and work-from-home requests shall be cancellable while pending. | M | ✅ |

### 7.14 Notification

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-116 | The system shall deliver operationally significant events to signed-in users in real time. | M | ✅ 13 event types |
| FR-117 | Every real-time notification shall also be persisted so it survives the recipient being offline. | M | ✅ |
| FR-118 | Notifications shall be classified by category (General, Leaves, Work Updates) and type (Project, Sprint, Leave, Task, System). | M | ✅ |
| FR-119 | The notification inbox shall support pagination, category filtering, time-range filtering of 7, 30 or 90 days, unread counting, and mark-one and mark-all read. | M | ✅ |
| FR-120 | Notifications shall link directly to the relevant record. | M | ✅ |
| FR-121 | Actionable notifications shall be approvable or rejectable directly from the inbox. | M | ✅ |
| FR-122 | Managers shall receive a live count of requests awaiting their decision. | M | ✅ |
| FR-123 | Notifications shall be deliverable by email as well as in-app, for approval actions performed away from the application. | S | ⚠️ Email is sent for Owner leave approval and Owner permission auto-approval only |

### 7.15 Authentication, session and access control

| ID | Requirement | Priority | Status |
|---|---|---|---|
| FR-124 | Users shall sign in with their unique 6-character code. | M | ✅ |
| FR-125 | Every sign-in shall open a session record capturing the time, IP address and device. | M | ✅ |
| FR-126 | A new sign-in shall close any previously open session for that user, recorded as replaced by a new login. | M | ✅ |
| FR-127 | Every session shall record why it ended: manual sign-out, expiry, inactivity, administrative action, or replacement. | M | ✅ |
| FR-128 | Sessions left open beyond token expiry shall be closed automatically within one minute. | M | ✅ |
| FR-129 | Ten minutes of inactivity shall pause the user's work, invalidate their access and sign them out. | M | ✅ |
| FR-130 | An expired user shall be prevented from continuing to navigate the application. | M | ✅ |
| FR-131 | Owner shall be able to browse all sessions, filtered by date range and reason. | M | ✅ |
| FR-132 | Owner shall be able to view a user's 24-hour activity storyline showing active and inactive periods. | M | ✅ |
| FR-133 | Owner shall be able to force a user out immediately, invalidating access and signing out their browser in real time. | M | ✅ |
| FR-134 | Every function shall be restricted to the roles entitled to perform it, enforced by the server. | M | ❌ **Enforced inconsistently — see CR-01** |
| FR-135 | Sign-in shall be protected against automated guessing by rate limiting and failed-attempt lockout. | M | ❌ See CR-02 |
| FR-136 | An active user shall not be signed out while working. | M | ❌ Sessions terminate every 10–15 minutes regardless of activity (CR-09) |

---

## 8. Business rules

Rules are stated as the business currently operates them. Rules marked **hard-coded** cannot be changed without a software release, which is itself a finding (CR-16).

### 8.1 Content and workflow

| ID | Rule | Configurability |
|---|---|---|
| BR-01 | A catalogue level cannot be deleted while it contains descendants. | Structural |
| BR-02 | Every Learning Objective must have a workflow schema. | Structural |
| BR-03 | A workflow stage may only begin when **every** one of its prerequisite stages is entirely complete. | Structural |
| BR-04 | Work is routed to a discipline, not a person; the discipline is determined by the Task Bank entry. | Structural |
| BR-05 | Work marked as Team-Leader work is queued to the leader; all other work enters the discipline backlog unassigned. | Structural |
| BR-06 | A Subject closes automatically when no incomplete work remains within it. | Structural |
| BR-07 | Rework may only go to a step explicitly configured as a permitted target. | Structural |
| BR-08 | Rework may only be initiated from work that is in progress. | Structural |
| BR-09 | Rework evidence is limited to 5 files of 10 MB, PDF or Office formats, with a total request of 60 MB. | **Hard-coded** |
| BR-10 | Learning objectives whose name contains the text "old" are excluded from all analytics. | **Hard-coded — defective, see CR-15** |

### 8.2 Roles and permissions

| ID | Rule |
|---|---|
| BR-11 | A Member may not assign work to anyone. |
| BR-12 | Work may only be assigned within its own discipline, except by Owner or Project Manager. |
| BR-13 | Only Owner or Project Manager may jump a workflow forward. |
| BR-14 | Only a Project Manager may change the workflow applied to a Learning Objective. |
| BR-15 | Only Owner or Project Manager may complete work on another user's behalf. |
| BR-16 | Only the Owner may view sessions, force users out, or view all staff leave balances. |
| BR-17 | Curriculum, workflow and user administration is reserved to Project Manager and Owner. |
| BR-18 | A dashboard may only be retrieved by a user of the matching role. |

### 8.3 Leave, permission and work from home

| ID | Rule | Configurability |
|---|---|---|
| BR-19 | The working week is Monday to Friday; weekends never count as leave. | **Hard-coded** |
| BR-20 | Sick leave exceeding 3 days requires a medical certificate. | **Hard-coded** |
| BR-21 | Emergency leave is unavailable during the blackout window before the annual reset. | Configurable |
| BR-22 | Borrowing from next year's balance is capped at 3 days by default and only inside a defined window. | Configurable |
| BR-23 | A request may not exceed the remaining balance, counting pending requests as consumed. | Structural |
| BR-24 | The approval chain is fixed at submission time and is unaffected by later reorganisation. | Structural |
| BR-25 | Each approver may record exactly one opinion per request. | Structural |
| BR-26 | The Owner is the final approver and Owner approval triggers a confirmation email. | Structural |
| BR-27 | Permission requests raised by the Owner are approved automatically and the CEO is notified. | Structural |
| BR-28 | Available permission slots are the ceiling less used and pending requests. | Structural |
| BR-29 | Work-from-home requests may not duplicate an existing active request for the same date, and may not be in the past. | Structural |
| BR-30 | Permission and work-from-home counters reset for all staff at midnight on the 21st of each month. | **Hard-coded** |
| BR-31 | Annual leave entitlements reset once per year on the configured date, journaled to prevent repetition. | Configurable |
| BR-32 | Only pending requests may be cancelled. | Structural |

### 8.4 Effort and productivity

| ID | Rule | Configurability |
|---|---|---|
| BR-33 | Effort is recorded as timed sessions, each closed with an explicit reason: complete, pause, flag, reassign, process change, skip, or session end. | Structural |
| BR-34 | Actual effort is the sum of recorded sessions belonging to active users; where none exists, the recorded task duration is used. | Structural |
| BR-35 | Expected effort resolves in order: the step's duration, then the Task Bank default, then the task's duration, then zero. | Structural |
| BR-36 | Points awarded: 3 for early, 2 for on time, 1 for late, 0 where no expectation exists. | **Hard-coded** |
| BR-37 | Report status labels: *Rollback* where the work is in rework or marked as rework; *Approved* where complete; *Hold* otherwise. | Structural |
| BR-38 | Productivity data excludes archived content, unassigned work and work assigned to archived users. | Structural |
| BR-39 | Rankings show the top 5 members, with the period selectable independently of the table period. | **Hard-coded** |

### 8.5 Access and session

| ID | Rule | Configurability |
|---|---|---|
| BR-40 | Sign-in credential is a unique 6-character code. There is no password. | Structural |
| BR-41 | Access expires after 10 minutes; the absolute session ceiling is 15 minutes. | **Hard-coded** |
| BR-42 | Ten minutes of inactivity pauses the user's work and signs them out. | **Hard-coded** |
| BR-43 | A new sign-in terminates the previous session, permitting one active session per user. | Structural |
| BR-44 | Every session must record why it ended. | Structural |

---

## 9. Non-functional requirements

| ID | Category | Requirement | Status |
|---|---|---|---|
| NFR-01 | Performance | The role landing dashboard shall load within 2 seconds. | ⚠️ Stated as a target in sprint documentation; not instrumented |
| NFR-02 | Performance | Large task boards shall be deliverable without buffering the whole payload. | ✅ A streaming endpoint exists for sprint boards |
| NFR-03 | Performance | Heavy analytical reports shall not degrade transactional performance. | ✅ Reporting uses purpose-built SQL with covering indexes |
| NFR-04 | Performance | List endpoints returning unbounded collections shall be paginated. | ⚠️ Applied to HR and reporting endpoints; users and subjects return complete collections (CR-38) |
| NFR-05 | Availability | Loss of the real-time channel shall not cause loss of notifications. | ✅ All notifications are persisted independently of delivery |
| NFR-06 | Availability | Transient database failures shall be retried automatically. | ✅ 5 retries at 5-second intervals |
| NFR-07 | Scalability | The system shall support horizontal scaling of the application tier. | ❌ Presence state is held in process memory with no real-time backplane (CR-36) |
| NFR-08 | Security | All functions shall be authorised server-side according to role. | ❌ **Critical — CR-01** |
| NFR-09 | Security | Sign-in shall resist automated guessing. | ❌ **Critical — CR-02** |
| NFR-10 | Security | Secrets shall not be stored in source-controlled configuration. | ❌ **Critical — CR-04** |
| NFR-11 | Security | Uploaded documents, including medical certificates, shall be accessible only to entitled users. | ❌ **Critical — CR-06** |
| NFR-12 | Security | The API definition shall not be publicly discoverable in production. | ❌ **Critical — CR-05** |
| NFR-13 | Security | Session credentials shall be protected against theft by injected scripts. | ❌ **Critical — CR-07** |
| NFR-14 | Auditability | All work state changes shall be attributable to an actor and a time. | ✅ |
| NFR-15 | Auditability | All changes to user records shall be journaled and viewable. | ✅ |
| NFR-16 | Auditability | All sign-in sessions shall be journaled with a termination reason. | ✅ |
| NFR-17 | Data retention | Records shall be archived rather than deleted so history is preserved. | ✅ Archive flags throughout |
| NFR-18 | Usability | Users shall not be interrupted by session expiry while actively working. | ❌ **CR-09** |
| NFR-19 | Usability | The application shall present a consistent visual language. | ⚠️ Four charting libraries and no theme layer (CR-30, CR-31) |
| NFR-20 | Accessibility | The application shall meet WCAG 2.1 AA. | ❌ Not assessed (CR-30) |
| NFR-21 | Localisation | The application shall be usable in Arabic with right-to-left layout. | ❌ English only, partial right-to-left in one screen (CR-29) |
| NFR-22 | Compatibility | The application shall function in current versions of Chrome and Edge. | ✅ |
| NFR-23 | Compatibility | The application shall be usable on tablet and mobile for approval workflows. | ❌ CR-46 |
| NFR-24 | Supportability | Failures shall be diagnosable from structured, correlated logs. | ❌ Plain-text error files only (CR-34) |
| NFR-25 | Supportability | The system shall expose a health endpoint for monitoring. | ❌ CR-34 |
| NFR-26 | Maintainability | Business thresholds shall be changeable without a software release. | ❌ Most are compiled constants (CR-16) |
| NFR-27 | Maintainability | Critical business logic shall be covered by automated tests. | ❌ Minimal coverage (CR-33) |
| NFR-28 | Deployability | Releases shall be gated by automated type-checking, tests and migration validation. | ❌ Type errors are suppressed; no pipeline found (CR-10, CR-47) |
| NFR-29 | Configurability | Environment-specific settings shall be supplied at deployment, not compiled in. | ❌ The API address is hard-coded in the client (CR-08) |

---

## 10. Data requirements and reporting

### 10.1 Master data owned by the business

| Data set | Owner | Volume characteristic |
|---|---|---|
| Curriculum catalogue (years, projects, terms, subject groups, subjects) | Project Manager | Hundreds of subjects |
| Production content (units, lessons, learning objectives) | Project Manager | Thousands of learning objectives |
| Workflow schemas, stages, steps | Project Manager | Tens of schemas |
| Task Bank | Project Manager | Tens of work types |
| Disciplines, teams, sections | Owner / HR | Tens of each |
| Users and entitlements | Owner / HR | Hundreds of users |
| Sprints | Project Manager | Tens per year |

### 10.2 Transactional data generated by the system

| Data set | Generated by | Retention expectation |
|---|---|---|
| Tasks | Workflow engine | Archived, never deleted |
| Task activity journal | Every state change | Permanent audit record |
| Effort sessions | Start/stop of work | Permanent — basis of all effort reporting |
| Comments | Users | Archived |
| Rework records, issues and attachments | Rework requests | Permanent |
| Leave, permission and work-from-home requests with opinions | Employees and approvers | Permanent HR record |
| Notifications | System events | Permanent |
| Sessions | Sign-in and sign-out | Permanent audit record |
| User change journal | Administrative edits | Permanent HR record |

### 10.3 Required reports

| Report | Audience | Purpose |
|---|---|---|
| Role landing dashboard | All | Situational awareness on sign-in |
| Project progress analytics | Project Manager, Owner | Learning-objective completion and status mix |
| Sprint progress analytics | Project Manager, Team Leader | Increment health and completion forecast |
| Discipline workload distribution | Managers | Where outstanding work is concentrated |
| Daily operational report | All (scoped) | Daily issue tracking with manager annotations |
| Productivity report and rankings | Managers | Effort against estimate; individual recognition |
| Per-user workload | Managers | Load balancing and reassignment |
| Hierarchical project report | Project Manager, Owner | Management reporting by date range |
| Staff leave balances | Owner | Absence liability and planning |
| Session audit | Owner | Access accountability |
| **Forward capacity forecast** | Owner, Project Manager | **Required but not delivered — CR-41** |
| **Estimate accuracy trend** | Project Manager | **Required but not delivered — CR-44** |

### 10.4 Data quality dependencies

| Dependency | Business consequence if unmet |
|---|---|
| Task Bank durations must be realistic | The entire productivity scoring model becomes meaningless; work with a zero estimate scores zero regardless of performance |
| Group colours must be distinguishable | Every workload chart becomes unreadable |
| Users must be archived rather than deleted when leaving | Historical effort and rankings lose their attribution |
| Learning objective names must not contain the text "old" | The item silently disappears from all analytics (CR-15) |
| Team leader and section assignments must be current | Approval requests route to the wrong manager |

---

## 11. Assumptions, dependencies and constraints

### 11.1 Assumptions

| ID | Assumption |
|---|---|
| A-01 | All users are known to HR and are issued a unique 6-character code. |
| A-02 | The working week is Monday to Friday for all staff, with no shift or regional variation. |
| A-03 | The payroll month boundary is the 21st, which is why permission and work-from-home counters reset then. |
| A-04 | One person works one task at a time, so effort sessions do not overlap. |
| A-05 | The organisational hierarchy (team leader, section) is kept current by HR, since approval routing depends on it. |
| A-06 | Production artefacts are stored in each discipline's own tooling; ATS references work, not deliverables. |
| A-07 | The system is used from within the corporate network or through the corporate reverse proxy. |
| A-08 | Users have a persistently connected browser, since inactivity detection relies on the real-time channel. |

### 11.2 Dependencies

| ID | Dependency | Impact if unavailable |
|---|---|---|
| D-01 | SQL Server (`SystemAdminDB_Test_v1`) | Total loss of service |
| D-02 | SMTP server | Approval confirmation emails are not sent; in-app notification continues |
| D-03 | SSRS server | The advanced report screen fails; all other reporting continues |
| D-04 | Corporate reverse proxy forwarding client IP headers | Session audit records the proxy address rather than the user's |
| D-05 | WebSocket connectivity | Real-time toasts and inactivity detection stop; persisted notifications still accumulate |

### 11.3 Constraints

| ID | Constraint | Business implication |
|---|---|---|
| C-01 | Authentication is a 6-character code with no password | Cannot satisfy a corporate authentication standard without change (CR-02) |
| C-02 | Access expires after 10 minutes, absolute ceiling 15 minutes | Users are interrupted repeatedly during a working day (CR-09) |
| C-03 | Only one active session per user | A user cannot work on two devices simultaneously |
| C-04 | Application tier cannot be scaled horizontally | Growth must be met by a larger single instance (CR-36) |
| C-05 | Most operational thresholds are compiled constants | Policy changes require a software release (CR-16) |
| C-06 | English-only interface | Constrains hiring and usability for Arabic-first staff (CR-29) |
| C-07 | Workflow schemas are not versioned | Historical reporting cannot reconstruct the process a completed item followed (CR-40) |
| C-08 | Sprints reference learning objectives, not tasks | Sprint scope cannot be fixed at a point in time; late-generated work is pulled in automatically |

---

## 12. Gap analysis and change requests

Each gap is stated as a business consequence with a proposed change request. Identifiers align with the technical findings in `docs/SYSTEM_DESCRIPTION.md`.

### 12.1 Critical — must be addressed before further feature investment

| CR | Gap | Business consequence | Recommendation |
|---|---|---|---|
| **CR-01** | Server-side authorisation is applied inconsistently. Many functions — including creating, editing and archiving **users** — are reachable without any role check. Role restrictions exist mostly in the user interface. | Any authenticated user, and in several cases any unauthenticated caller who knows the address, can perform administrative operations. The role model that the business relies on is not actually enforced. This undermines FR-009, FR-062, FR-091 and FR-134. | Introduce a default policy requiring authentication on every endpoint, then apply explicit role policies. Treat as a **release blocker**. |
| **CR-02** | Sign-in uses a 6-character code with no rate limiting and no lockout. | The credential space is small enough to guess by automation. Combined with CR-01, a successful guess grants broad access. | Add rate limiting and failed-attempt lockout immediately. Plan migration to corporate single sign-on. |
| **CR-03** | Token validation does not check issuer or audience. | A token signed with the shared key from any source is accepted. | Enable issuer and audience validation. |
| **CR-04** | The signing key, encryption key, database credentials and mail credentials are held in source-controlled configuration. | Anyone with repository access can impersonate any user and read the database. | Move to a secret store and **rotate the exposed keys**. |
| **CR-05** | The full API definition is published in every environment. | The entire attack surface is discoverable, which compounds CR-01. | Restrict to non-production or to authenticated users. |
| **CR-06** | Rework attachments and **medical certificates** are stored in the publicly served folder, and the attachment download requires no authentication. | Employee health documents may be retrievable by anyone who can reach the server. This is a probable data-protection breach. | Move uploads outside the served folder, serve only via an authorised endpoint, and consider encryption at rest. Treat as **urgent**. |
| **CR-07** | The session credential is stored in browser local storage. | Any injected script can steal a live session. | Hold the credential in memory only, or move to secure cookies with cross-site request protection. |

### 12.2 High — materially affecting business operation

| CR | Gap | Business consequence | Recommendation |
|---|---|---|---|
| **CR-08** | The client is compiled with a hard-coded reference to a developer's local API address. | A production build points at the wrong server; releases depend on a manual edit. | Supply the address at build time. |
| **CR-09** | Sessions expire after 10 minutes with no automatic renewal, so an actively working user is signed out repeatedly. | The most visible daily complaint in a system whose users spend hours on task boards. It likely drives workarounds and lost effort data. | Implement transparent renewal and raise the working session length; the inactivity watchdog already covers the abandoned-desk case independently. |
| **CR-10** | Type errors do not prevent a release. | Defects that the tooling already detects reach production. | Fix outstanding errors and make type-checking a release gate. |
| **CR-11** | Some screens send no credential with their requests. | Tightening CR-01 will break those screens unless this is fixed first. | Route all requests through one layer that attaches the credential and handles renewal. |
| **CR-12** | Several leave endpoints report success without doing anything. | A user or integration believes an update or deletion succeeded when it did not. | Implement or return an explicit "not implemented" response. |
| **CR-13** | A duplicate legacy leave interface exists under a second address. | Two contracts with different validation can produce inconsistent HR records. | Retire the legacy interface. |
| **CR-14** | Three functional areas share one address prefix. | Fragile routing and an API that is difficult to reason about or document. | Separate the prefixes. |
| **CR-15** | All analytics silently exclude any learning objective whose name contains the text "old". | Legitimate content — for example "Gold coins" or "Household items" — disappears from every chart and report without warning. **Progress and productivity figures are wrong for such items.** | Replace with an explicit deprecation flag and migrate existing data. |
| **CR-16** | Working week, sick-certificate threshold, scoring scale, monthly reset day, session lifetimes and upload limits are compiled constants. | Any HR or operational policy change requires a software release, so policy is effectively frozen between releases. | Move operational thresholds into configuration or an administrable settings screen. |

### 12.3 Medium — data quality, maintainability and completeness

| CR | Gap | Business consequence | Recommendation |
|---|---|---|---|
| **CR-17** | Superseded model classes and an empty file remain in the codebase. | New engineers are misled about how the system works, increasing the cost and risk of every change. | Remove dead code. |
| **CR-18** | An intended team-leader field on Team is silently not saved. | A relationship the code appears to model does not exist, so any report built on it would be wrong. | Remove or implement, and declare which representation is authoritative. |
| **CR-19** | A reporting table has two conflicting columns for the same fact. | Risk of reading the wrong column and mis-attributing manager notes. | Reconcile with a corrective migration. |
| **CR-20** | Leave balances live on the same record as user identity, and no historical balances are retained. | Prior-year entitlements and usage cannot be reconstructed, which is a plausible audit requirement. | Extract balances into a year-scoped table. |
| **CR-21** | The approval table permits a record that belongs to no request, or to several. | Orphaned or ambiguous approval records can enter the HR audit trail. | Add a database constraint or restructure. |
| **CR-22** | Two different curriculum vocabularies are documented, only one of which is built. | Business and engineering use different words for the same things, causing repeated miscommunication. | Retire the unimplemented model or align the vocabulary. |
| **CR-23** | Existing documentation states incorrect session lifetimes, an incorrect interface address, and an incorrect screen count. | Decisions are taken on stale facts. | Correct the documents and stamp each with the version it was verified against. |
| **CR-24** | Request validation is inline and inconsistent, with no common error contract. | Users receive inconsistent error messages, and validation gaps are easy to introduce. | Adopt a validation framework with a standard error shape. |
| **CR-25** | Several functions bypass the service layer entirely. | Their business rules cannot be reused or tested, so behaviour can diverge between screens. | Move into services. |
| **CR-26** | Section Head and Member dashboards were documented as incomplete; the endpoints now exist but the content has not been signed off. | Two of five roles may be landing on a screen that does not meet their needs. | Review against the original acceptance criteria and close formally. |
| **CR-27** | Sprint analytics returns the same summary twice under two names. | Consumers may believe they are seeing two measures when they are seeing one. | Implement the distinct measure or remove the duplicate. |
| **CR-28** | Three built and working report screens are unreachable because their navigation entries are disabled. | Delivered capability is providing no value. | Decide to restore or retire, and act. |
| **CR-29** | No multi-language support, despite Arabic content and one screen already forcing right-to-left layout. | Limits usability and hiring for Arabic-first staff at an Arabic-market publisher. | Introduce localisation and a layout-direction strategy if Arabic is required. |
| **CR-30** | No theme layer, no dark mode, and no accessibility assessment. Analytics colours come from free-text entries. | Charts may be unreadable; no assurance of accessibility compliance. | Add a theme with defined tokens; validate colour contrast; run an accessibility audit. |
| **CR-31** | Four charting libraries and four date libraries are in use. | Slower pages and visually inconsistent reporting. | Standardise on one of each. |
| **CR-32** | No client-side data caching, retry or deduplication. | Repeated identical requests, slower screens, and a poor experience on unreliable connections. | Adopt a query cache; it also provides one place to solve CR-09 and CR-11. |

### 12.4 Lower priority — engineering practice

| CR | Gap | Business consequence | Recommendation |
|---|---|---|---|
| **CR-33** | The highest-risk logic — work generation, leave arithmetic, productivity scoring — is not covered by automated tests. | A defect in work generation could stall the entire pipeline, or a leave arithmetic defect could misstate entitlements, with no safety net. | Prioritise tests for these three areas. |
| **CR-34** | Logging is plain text with no correlation, no health endpoint and no metrics. | Incidents take longer to diagnose; degradation of the automated background processes goes unnoticed. | Add structured logging, correlation, health endpoints and worker metrics. |
| **CR-35** | Database migrations and one-off historical repair routines run automatically on every start. | Risk of contention in a multi-instance deployment, and routines needed once now run indefinitely. | Move migrations to a deployment step and retire the repair routines. |
| **CR-36** | Presence tracking is held in a single instance's memory with no coordination layer. | The system cannot be scaled out; doing so would break inactivity detection and force-logout. | Add a coordination layer before scaling. |
| **CR-37** | Naming inconsistencies and misspellings persist in the code and interface addresses, including a misspelled role name. | Ongoing friction and integration confusion. | Correct in a coordinated change. |
| **CR-38** | Some lists return every record with no paging. | Screens will degrade as the organisation and catalogue grow. | Apply paging consistently. |
| **CR-39** | Archived-record filtering is applied manually in every query. | A single omission silently exposes archived data in a report. | Apply filtering centrally with an explicit opt-out. |

### 12.5 Strategic opportunities — new business capability

| CR | Opportunity | Business value | Priority |
|---|---|---|---|
| **CR-40** | **Version workflow schemas.** Pin the schema version to each learning objective at creation. | Historical reporting can reconstruct the process a completed item followed, making throughput comparison across process changes valid. Currently a mid-flight process change invalidates historical comparison silently. | High |
| **CR-41** | **Forward capacity forecasting.** The system already holds expected durations, historical actuals, discipline membership and approved absence calendars. Combining them yields genuine capacity forecasting. | The highest-value capability not yet built. Answers "can we take this on and when will it land" from data the business already owns. | **Highest** |
| **CR-42** | **Automatic ageing and escalation.** Alert when work exceeds its expected duration or waits unclaimed beyond a threshold. | Converts the system from reactive to proactive. Today escalation depends entirely on a member remembering to flag. | High |
| **CR-43** | **Email-actionable approvals.** Mail is already configured; approval is the most time-sensitive and least screen-hungry workflow. | Removes the main source of approval delay for managers away from their desk. | Medium |
| **CR-44** | **Estimate recalibration.** Feed observed averages back into default durations. | Every downstream estimate, forecast and productivity score improves automatically over time. | Medium |
| **CR-45** | **External contributor experience.** The data model already distinguishes internal from external, but there is no scoped experience for external contributors. | Enables safe use of freelance and vendor capacity without exposing internal data. | Medium |
| **CR-46** | **Mobile approval experience.** | Managers approve from anywhere, shortening the leave and permission cycle. | Medium |
| **CR-47** | **Deployment pipeline.** No container definition, continuous integration configuration or infrastructure-as-code was found. | Without a pipeline, the quality gates recommended in CR-10, CR-33 and CR-35 cannot be enforced. | High (enabler) |

---

## 13. Risks

| ID | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R-01 | Unauthorised access or modification through unprotected functions (CR-01) | High | **Severe** — data integrity and confidentiality | Implement CR-01 as a release blocker; audit recent activity journals for unexpected administrative actions |
| R-02 | Exposure of employee medical certificates (CR-06) | Medium | **Severe** — data-protection breach with regulatory exposure | Implement CR-06 urgently; assess whether historical exposure requires notification |
| R-03 | Credential compromise from committed secrets (CR-04) | Medium | **Severe** | Rotate keys and move to a secret store |
| R-04 | Reporting understates progress because of the "old" name exclusion (CR-15) | **High** — a common English substring | High — decisions taken on incorrect figures | Implement CR-15 and re-baseline affected reports |
| R-05 | Users abandon the system or lose work because of constant session expiry (CR-09) | High | Medium — adoption and effort-data quality | Implement CR-09 |
| R-06 | A defect in automated work generation stalls the pipeline, with no test coverage to catch it (CR-33) | Medium | High — production halt | Add tests for work generation before further changes to it |
| R-07 | Historical throughput comparison is invalid because processes change without versioning (CR-40) | Medium | Medium — decisions on invalid trends | Implement CR-40 |
| R-08 | Growth cannot be met because the application cannot scale out (CR-36) | Medium | Medium | Address before headcount or catalogue growth demands it |
| R-09 | Business rules cannot be changed at the pace policy requires (CR-16) | High | Medium — HR and operations blocked on release cycles | Externalise thresholds |
| R-10 | Knowledge loss because documentation contradicts the built system (CR-22, CR-23) | High | Medium — onboarding cost and wrong decisions | Adopt this document set as the maintained baseline |
| R-11 | Approvals route to the wrong manager after a reorganisation | Medium | Medium | The chain is frozen at submission, which is correct; add a report of requests whose recorded approvers have changed role |
| R-12 | Effort data is distorted by users leaving tasks running | Low | Medium | Already mitigated by the inactivity watchdog; monitor the frequency of session-reason closures |

---

## 14. Implementation roadmap

### Phase 0 — Security remediation (immediate; blocks further feature work)

| Item | Requirement |
|---|---|
| CR-01 | Enforce server-side authorisation on every function |
| CR-06 | Secure uploaded documents, especially medical certificates |
| CR-04 | Move and rotate secrets |
| CR-02 | Rate limiting and lockout on sign-in |
| CR-03 | Enable issuer and audience validation |
| CR-05 | Restrict the API definition in production |
| CR-07 | Remove the session credential from browser storage |

**Exit criteria:** an independent security review confirms no function is reachable without appropriate authorisation, and no employee document is retrievable without entitlement.

### Phase 1 — Operational correctness

| Item | Requirement |
|---|---|
| CR-15 | Remove the defective content-name exclusion and re-baseline reports |
| CR-09 | Transparent session renewal and a working session length |
| CR-08 | Environment-supplied API address |
| CR-11 | Centralised request layer with consistent credentials |
| CR-12 | Remove misleading success responses |
| CR-10 | Make type-checking a release gate |
| CR-33 | Tests for work generation, leave arithmetic and productivity scoring |
| CR-47 | Deployment pipeline enforcing the above gates |

### Phase 2 — Business agility and completeness

| Item | Requirement |
|---|---|
| CR-16 | Externalise operational thresholds into an administrable settings screen |
| CR-26 | Sign off Section Head and Member dashboards |
| CR-28 | Restore or retire the report screens |
| CR-20 | Year-scoped leave balances with history |
| CR-40 | Workflow schema versioning |
| CR-34 | Structured logging, health endpoints and worker monitoring |

### Phase 3 — New business capability

| Item | Requirement |
|---|---|
| CR-41 | Forward capacity forecasting |
| CR-42 | Automatic ageing and escalation |
| CR-44 | Estimate recalibration from observed effort |
| CR-43 | Email-actionable approvals |
| CR-46 | Mobile approval experience |
| CR-45 | External contributor experience |

### Phase 4 — Platform and experience

| Item | Requirement |
|---|---|
| CR-29 | Localisation and right-to-left layout |
| CR-30 | Theme layer and accessibility compliance |
| CR-31, CR-32 | Library consolidation and client-side caching |
| CR-36 | Horizontal scalability |
| CR-17, CR-18, CR-19, CR-21, CR-25, CR-37, CR-38, CR-39 | Technical debt reduction |

---

## 15. Glossary

| Term | Definition |
|---|---|
| **Academic Year** | The top level of the curriculum catalogue. |
| **Archive** | Marking a record inactive while preserving it. ATS prefers archiving to deletion throughout. |
| **Backlog** | The pool of generated but unclaimed work belonging to a discipline. |
| **Curriculum Project** | The second catalogue level, beneath Academic Year. Distinct from the everyday sense of "project", which in ATS usually means a Subject. |
| **Discipline** | A production specialism. Modelled as a **Group**. |
| **Flag** | A member's escalation of a blocked or problematic task to their Team Leader. |
| **Group** | See Discipline. Owns work before a person does; carries a colour used across all analytics. |
| **Jump** | A manager moving a workflow forward past intervening stages. Restricted to Owner and Project Manager. |
| **Learning Objective** | The unit of production. Carries the workflow, generates the tasks, and is the level at which sprints and progress are measured. |
| **Lesson** | The grouping between Unit and Learning Objective. |
| **Node** | Technical term for a workflow **Stage**. |
| **Opinion** | One approver's recorded decision on a leave, permission or work-from-home request, with an optional comment. |
| **Permission** | A short, timed absence on a single date — work assignment, early departure, late arrival or departure. |
| **Rollback** | Rework. Sending work back to a pre-approved earlier step with issue notes and evidence. |
| **Schema** | A reusable, named production process. |
| **Section** | An oversight layer spanning several Groups, headed by a Section Head. Determines HR approval scope. |
| **Sprint** | A time-boxed delivery increment. References Learning Objectives, not tasks. |
| **Stage** | A phase of a production process. May require several predecessors and release several successors. |
| **Step** | A discrete unit of work within a Stage, with its own expected duration and priority. |
| **Subject** | The leaf of the catalogue and the level at which the business declares work complete. Often called a "project" in everyday use. |
| **Subject Group** | A grouping of related Subjects within a Term. |
| **Task** | One instance of work, generated by the system, for one Learning Objective at one Step. |
| **Task Bank** | The catalogue of work types, each bound to a discipline and a default duration. |
| **Team** | An administrative reporting unit of people. Distinct from Group, which is a discipline. |
| **Term** | A time window within a Curriculum Project. |
| **Unit** | The first level of decomposition beneath a Subject. |
| **Work session** | A timed period of effort on one task by one user, closed with an explicit reason. |

---

## 16. Appendix — requirements traceability

### 16.1 Objectives to requirements

| Business objective | Supporting requirements | Status |
|---|---|---|
| BO-01 Eliminate manual coordination | FR-021 … FR-027, FR-025 | ✅ |
| BO-02 Enforce consistent process | FR-006, FR-010 … FR-019 | ✅ |
| BO-03 Complete traceability | FR-035, FR-046 … FR-051, FR-090, FR-127 | ✅ |
| BO-04 Measure effort | FR-030, FR-031, FR-038, FR-076 … FR-079 | ✅ |
| BO-05 Role-appropriate visibility | FR-037, FR-063 … FR-069, FR-080 | ⚠️ CR-26 |
| BO-06 Time-boxed delivery | FR-058 … FR-062, FR-070 … FR-073 | ✅ |
| BO-07 Unified availability administration | FR-092 … FR-115 | ✅ |
| BO-08 Prompt action on requests | FR-116 … FR-123 | ✅ |
| BO-09 Access accountability | FR-124 … FR-133 | ✅ |
| BO-10 Capacity forecasting | — | ❌ CR-41 |
| BO-11 Detect stalled work | FR-039 | ❌ CR-42 |
| BO-12 Improve estimation | — | ❌ CR-44 |

### 16.2 Requirements not met, by cause

| Requirement | Cause | Change request |
|---|---|---|
| FR-009, FR-062, FR-091, FR-134, NFR-08 | Authorisation not enforced server-side | CR-01 |
| FR-052, FR-104, NFR-11 | Uploads publicly served and unauthenticated | CR-06 |
| FR-135, NFR-09 | No rate limiting or lockout | CR-02 |
| FR-136, NFR-18 | No transparent session renewal | CR-09 |
| FR-020 | Workflow schemas not versioned | CR-40 |
| FR-039 | No ageing or escalation | CR-42 |
| FR-066, FR-067 | Dashboard content not signed off | CR-26 |
| FR-082 | Navigation entries disabled | CR-28 |
| FR-123 | Email notification limited to two scenarios | CR-43 |
| NFR-01, NFR-24, NFR-25 | No instrumentation or observability | CR-34 |
| NFR-04, NFR-38 | Paging applied inconsistently | CR-38 |
| NFR-07 | Presence state held in process memory | CR-36 |
| NFR-10, NFR-12, NFR-13 | Security configuration | CR-04, CR-05, CR-07 |
| NFR-19, NFR-20, NFR-21 | No theme, accessibility or localisation layer | CR-29, CR-30, CR-31 |
| NFR-26 | Business thresholds compiled in | CR-16 |
| NFR-27 | Minimal test coverage | CR-33 |
| NFR-28, NFR-29 | No release gates; environment values compiled in | CR-08, CR-10, CR-47 |

### 16.3 Summary

| Measure | Count |
|---|---|
| Functional requirements documented | 136 |
| Fully implemented | 121 (89%) |
| Partially implemented | 6 (4%) |
| Not implemented | 9 (7%) |
| Non-functional requirements documented | 29 |
| Non-functional requirements met | 9 (31%) |
| Business rules documented | 44 |
| Change requests raised | 47 |
| — of which critical (security) | 7 |
| — of which high | 9 |
| — of which medium | 16 |
| — of which lower priority | 7 |
| — of which strategic opportunities | 8 |

**Headline assessment:** the system delivers its functional intent to a high standard — the workflow automation, audit and HR capabilities are substantial and well-realised. The gap is almost entirely non-functional, concentrated in **security enforcement**, **operational configurability** and **engineering practice**. Phase 0 of the roadmap should be treated as a prerequisite to any further feature investment.

---

*Prepared from a complete scan of the delivered solution. Where existing project documentation conflicted with the source code, the source code was treated as authoritative; those conflicts are recorded as CR-23. This document should be maintained alongside `docs/SYSTEM_DESCRIPTION.md` and re-verified at each release.*
