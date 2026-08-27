---
name: project-documentation
description: >-
  Creates and maintains ATS stakeholder documentation from source code.
  Use when the user asks for a Project Brief, PRD, BRD, system description,
  product docs, requirements, or to document / update / regenerate project
  documentation. Also use when answering what ATS is for, who uses it, or
  what is in or out of scope.
---

# Project documentation

Canonical docs live in `docs/`. Source code is authoritative when docs and code disagree.

| File | Audience | Contains |
|---|---|---|
| `docs/PROJECT_BRIEF.md` | Stakeholders | Goal, context, audience, scope, stakeholders, timeline, success criteria |
| `docs/PRD.md` | Product / engineering | Features, user and functional requirements, constraints, acceptance, traceability |
| `docs/BRD.md` | Business | Business rules, use cases, gap analysis, functional catalogue |
| `docs/SUPPORTING_NOTES.md` | Engineering | Historical root notes (not authoritative) |

`BRD.md` and `SYSTEM_DESCRIPTION.md` may be missing; regenerate them when asked. Do not invent parallel files (`README_PRODUCT.md`, `REQUIREMENTS.md`, etc.).

Older notes are grouped in `docs/SUPPORTING_NOTES.md` (former root markdown). Prefer `docs/` plus code.

## When invoked

1. Identify the request: **create**, **update after a change**, or **answer from docs**.
2. Read every file that currently exists under `docs/`.
3. Verify against code using [sources.md](sources.md). If `graphify-out/graph.json` exists and the graphify skill is available, query it for architecture questions; still confirm behaviour in code.
4. Write or patch only the documents the user asked for. If they said "document the project" with no type, produce or refresh **Brief + PRD**. Offer BRD and system description; do not write them unless asked or they already exist and are stale.
5. Keep IDs stable (`F-01`, `UR-M-01`, `FR-001`, `AC-01`, `SC-01`). Add new IDs; do not renumber.
6. Mark status honestly: **Delivered** · **Partial** · **Not delivered**. Never describe unimplemented work as shipped.

## Create / full regenerate

Copy this checklist:

```
- [ ] Read existing docs/
- [ ] Scan code (see sources.md)
- [ ] Draft or replace the requested doc(s) using templates.md
- [ ] Cross-check IDs and companion links
- [ ] List residual gaps (security, unmet success criteria) without inflating scope
```

Follow the section order in [templates.md](templates.md). Match the voice of the current Brief/PRD: short sentences, tables over prose, organisation = Selah El-Telmeez, system name = Automated Task System (ATS).

Facts that must stay true unless code proves otherwise:

- ATS is an **internal production-management platform**, not a generic tracker and not an LMS.
- Work is generated from a **workflow schema** attached to a **learning objective**.
- Curriculum (post-2026 hierarchy): Academic Year → Curriculum Project → Term → Subject Group → Subject → Unit → Lesson → Learning Objective → Tasks.
- Roles: Owner, Project Manager (`ProjectManger` in code), Section Head, Team Leader, Member.
- Group = production discipline; Team = reporting unit; Section = oversight across groups.

## Update after a change

When documenting a feature, fix, or behaviour change:

1. Find the affected feature / FR / AC rows in `docs/PRD.md` and the matching Brief scope or success-criteria row.
2. Patch those rows in place. Update status, acceptance steps, and the traceability table.
3. If the change is new capability, add a feature row and downstream IDs; mention it in Brief §4 (scope) and §7 (success) when it is user-visible.
4. Touch BRD / system description only if they exist and the change alters a business rule or a technical mechanic.
5. Bump the document **Date**; add a Document control row. Do not rewrite the whole file for a local change.

## Answer from docs

For "what is this system / who is it for / what's in scope":

1. Answer from `docs/PROJECT_BRIEF.md` first, then `docs/PRD.md`.
2. If those files are missing or clearly stale versus code, say so and offer to regenerate.
3. Do not dump the full documents into chat. Summarise, then point to the file.

## Do not

- Create unsolicited markdown at repo root.
- Copy stack trivia into the Brief (that belongs in the system description).
- Treat UI-only role checks as server authorisation.
- Update the 2022–era Year → Project → Unit curriculum path as current; the 2026 catalogue refactor supersedes it.
- Copy screen or controller counts from `docs/SUPPORTING_NOTES.md` into Brief/PRD; recount from code.
