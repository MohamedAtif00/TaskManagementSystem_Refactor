# Sprints and Projects Analysis

## Project Analysis

### Project Purpose and Scope
- The Automated Task System (ATS) is a full-stack internal platform for curriculum production workflows, not a generic task tracker.
- It combines curriculum lifecycle management, workflow automation, sprint delivery, reporting, and HR approval processes in one system.
- The core domain connects `Year -> Project -> Unit -> Lesson -> Learning Objective` to workflow-driven task execution.

### Architecture Analysis
- Backend follows `Controllers -> Services -> DataContext` with standardized API response wrapping.
- Frontend is Next.js (Pages Router) with role-aware rendering and API-driven dashboards.
- Real-time updates are delivered through SignalR for operational events (task assignments and approval-related notifications).
- System design uses reusable workflow schemas with nodes and steps that generate and propagate tasks based on prerequisites.

### Feature and Capability Analysis
- Graph-based workflow execution is the main differentiator from linear status-driven systems.
- Project lifecycle management supports active, on-hold, and closed states with reporting surfaces.
- Sprint planning and analytics are integrated with project execution rather than handled in a separate tool.
- Role hierarchy affects both backend behavior and frontend visibility, creating consistent permission boundaries.

### Delivery Surface Analysis (Projects + Sprints UI)
- Project module includes list and details views for project-level monitoring and navigation.
- Sprint module includes sprint list/details and per-learning-object board/sheet task views.
- Reporting layer includes project overview, summaries, and advanced report pages for cross-project analysis.

## Sprint Analysis

### Sprint: Role-Based Home Page Dashboard

#### Goal Analysis
- Primary sprint outcome is role-specific home dashboards so users only see relevant data and actions.
- Scope includes both backend dashboard endpoints and frontend role-based rendering components.

#### Requirements Analysis
- Existing dashboards cover Owner/Coordinator and Team Leader roles.
- Missing parts identified: Section Head and Member dashboard APIs and UI components.
- Role mapping must be fully consistent across auth state, API authorization, and UI rendering.

#### Backlog and Effort Analysis
- Backend work focuses on creating new endpoints for Section Head and Member dashboards and validating role delivery from auth profile.
- Frontend work focuses on role routing in home page, new dashboard components, API integration methods, and loading/error states.
- Testing is explicitly split into manual role validation and integration checks for API/auth flow.

#### Risk Analysis
- Highest risks are incorrect role mapping and unavailable backend endpoints.
- Moderate risks include partial/missing dashboard data and user-facing performance degradation.
- Mitigation strategy centers on enum/value verification, mock data fallback, and robust loading/error UI.

#### Success Metrics Analysis
- All five roles should receive appropriate dashboards with zero unauthorized content exposure.
- Dashboard render time target is within two seconds.
- No dashboard-related console/runtime errors are expected at release quality.

### Sprint Analytics API Analysis

#### Endpoint and Contract
- `GET /sprints/{sprintId}/analytics/overview` is designed as a pre-aggregated analytics endpoint.
- Contract includes tags distribution, task summary, learning-objective summary, and sprint summary.
- Response envelope follows a shared `ResponseService<T>` shape with `error` and optional `message`.

#### Data Semantics
- `taskSummary` and `sprintSummary` provide status-based counts (`active`, `completed`, `rollback`, `flagged`, `notStarted`, `total`).
- `loSummary` captures completion progression for learning objectives.
- `tags` represent categorized learning-objective workload with visual metadata for chart rendering.

#### Frontend Consumption Analysis
- Frontend behavior depends on strict shape validity: loading state, error state, empty state, then chart rendering.
- The API is suitable for overview dashboards because it minimizes client-side aggregation cost.
- Contract clarity and status code coverage reduce ambiguity for both UI and test implementation.

#### Extracted Query (Actual Implementation) + Description

**HTTP Query (actual endpoint used in code):**

```http
GET /sprints/analytics/overview?sprintId={sprintId}&timePeriod={timePeriod}
```

**Query Parameters Description:**
- `sprintId` (required): the sprint identifier to calculate analytics for.
- `timePeriod` (optional): date filter for tag distribution where:
  - `1` = Today
  - `2` = Last Week
  - `3` = Last Month
  - `4` = All Time (default behavior)

**What this query returns:**
- `taskSummary`: unfiltered global task-status counts for the sprint.
- `tags`: grouped task counts by group/tag, filtered by optional time period.
- `loSummary`: unfiltered learning-objective completion/not-started/in-process counts.
- `sprintSummary`: currently same values as `taskSummary`.

**Core SQL Query (used for task summary in backend service):**

```sql
SELECT
    Completed = SUM(CASE WHEN t.Status = @done THEN 1 ELSE 0 END),
    Active = SUM(CASE WHEN t.Status IN (@todo, @doing) THEN 1 ELSE 0 END),
    RollbackCount = SUM(CASE WHEN t.IsRollback = 1 THEN 1 ELSE 0 END),
    Flagged = SUM(CASE WHEN t.Flagged = 1 THEN 1 ELSE 0 END)
FROM Tasks t
INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId AND lo.Archived = 0
INNER JOIN SprintLearningObjectives slo ON slo.LearningObjectiveId = lo.Id AND slo.SprintId = @sprintId
WHERE t.Archived = 0
  AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%';
```

**SQL Description:**
- Aggregates tasks linked to learning objectives in the selected sprint.
- Excludes archived tasks and archived learning objectives.
- Excludes legacy learning objectives whose names contain `"old"`.
- Produces the raw counts that are mapped to `Completed`, `Active`, `Rollback`, and `Flagged`.

## Sources Used for Extraction
- `PROJECT_EXPLANATION_PROMPT.md`
- `FRONTEND_SCREENS_LIST.md`
- `AutomatedTaskSystem.UI/client/SPRINT_Role_Based_Home_Page.md`
- `AutomatedTaskSystem.UI/client/SPRINT_OVERVIEW_API_REQUIREMENTS.md`
