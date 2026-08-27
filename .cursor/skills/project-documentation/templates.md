# Document templates

Reuse the heading order and ID schemes already used in `docs/`. Fill from code; do not leave placeholder prose.

Every file starts with:

```markdown
# <Document title>
## Automated Task System (ATS) — Task Management System v1.0

| Field | Value |
|---|---|
| **Document type** | … |
| **System** | Automated Task System (ATS) / Task Management System v1.0 |
| **Organisation** | Selah El-Telmeez — Educational Content Production |
| **Version** | 1.0 |
| **Date** | YYYY-MM-DD |
| **Status** | For review |
| **Companion documents** | `docs/PROJECT_BRIEF.md`, `docs/PRD.md`, `docs/BRD.md`, `docs/SYSTEM_DESCRIPTION.md` |
```

Priority legend (PRD): **M** = Must have · **S** = Should have · **C** = Could have  
Status legend: **Delivered** · **Partial** · **Not delivered**

---

## Project Brief (`docs/PROJECT_BRIEF.md`)

1. Goal — one job statement plus numbered outcomes. State what ATS is **not**.
2. Business context — organisation, Group/Team/Section, curriculum tree, problem before ATS, solution, current-state size.
3. Target audience — table of five roles + HR; explicit non-audience.
4. Scope — in-scope table, out-of-scope table, assumptions.
5. Stakeholders — system users, non-user stakeholders, sign-off table.
6. Timeline — delivery eras from migrations/git, then forward roadmap phases.
7. Success criteria — `SC-nn` table with how-we-know and current (Met / Partially met / Not met). Release-quality bar. What "done" for v1.0 means.

---

## PRD (`docs/PRD.md`)

Include Document control (version table) and a table of contents.

1. Product summary — one paragraph.
2. Features — `F-nn` map (ID, name, priority, status, primary users) then short descriptions.
3. User requirements — per role `UR-<role>-nn` plus `UR-X-nn` cross-cutting.
4. Functional requirements — numbered `FR-nnn` grouped by area (catalogue, schema, generation, execution, assignment, rework, intervention, sprints, dashboards, reporting, org admin, leave, permission/WFH, notification, auth/session).
5. Constraints — business, technical, security gaps, out-of-scope.
6. Acceptance criteria — `AC-nn` suites with Given/When/Then or numbered checks tied to FRs.
7. Traceability — Feature → user requirements → FRs → AC. Counts footer.

Existing ID prefixes: `UR-M`, `UR-TL`, `UR-SH`, `UR-PM`, `UR-O`, `UR-X`.

---

## BRD (`docs/BRD.md`)

1. Document control
2. Business objectives and success
3. Organisation and roles
4. Current vs target operating model
5. Business rules (numbered, testable)
6. Use cases / scenarios per role
7. Functional catalogue (can reference PRD feature IDs)
8. Data and integrations (SQL Server, SMTP, SignalR, SSRS, reverse proxy `ats.stp.local`)
9. Assumptions, dependencies, constraints
10. Gap analysis and phased roadmap (Phase 0 security → Phase 4 platform)
11. Risks and open questions

---

## System description (`docs/SYSTEM_DESCRIPTION.md`)

1. Executive summary
2. Business context and purpose
3. Solution architecture (API + Next.js client + SQL Server + workers + hub)
4. Technology stack
5. Domain model (entities and the curriculum/workflow/task graph)
6. Core mechanics (task generation, progression, rollback, sprints, HR approvals, notifications, sessions)
7. Security, authentication, session management — include known gaps
8. Reporting and background jobs
9. Frontend surface (routes/roles; do not trust stale screen counts)
10. Gaps and later work

---

## Voice

- Reconstruct from the **implemented** product, then label gaps.
- Prefer tables. One idea per sentence.
- Spell the code typo `ProjectManger` when referring to the enum; say "Project Manager" in stakeholder prose.
- Do not claim SSO, payroll, LMS, or mobile apps.
