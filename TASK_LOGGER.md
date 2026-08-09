# Task Logger — Documentation

## What is Task Logger?

**Task Logger** is a read-only TMS screen that shows **member productivity** based on real tasks in the database (not manual entry).

It answers:

- Who worked on which tasks (LO / subject / task name)?
- How long did they take (actual minutes)?
- How does that compare to expected duration?
- How many points did each member earn?
- Who ranks highest (All Time / This Week / This Month)?

**UI route:** `/task-logger`  
**API prefix:** `/task-logger`  
**Source code:**

| Layer | Path |
|--------|------|
| Controller | `AutomatedTaskSystem/Controllers/TaskLoggerController.cs` |
| Service | `AutomatedTaskSystem/Services/TaskLogger/TaskLoggerService.cs` |
| SQL | `AutomatedTaskSystem/Services/TaskLogger/TaskLoggerSql.cs` |
| Frontend | `AutomatedTaskSystem.UI/client/src/components/taskLogger/` |

There is **no** “Add New Task” form. Everything is derived from existing TMS tables.

---

## What data the screen shows

### 1. Stats cards

| Card | Meaning |
|------|---------|
| **Total Tasks** | Count of tasks in the current filter window |
| **Total Time (min)** | Sum of actual minutes for those tasks |
| **Rollback Tasks** | Count where status = Rollback |
| **Total Points** | Sum of productivity points |

### 2. Member Rankings (by Points)

Aggregates **points per assigned member** (active users only), ordered highest first.

Time range for rankings (independent of the table date filters):

| Range | Meaning |
|-------|---------|
| **All Time** | No date bound on rankings |
| **This Week** | Monday of current week → today |
| **This Month** | 1st of current month → today |

### 3. Filters + Tasks Log table

| Column | Source |
|--------|--------|
| **Date** | Last Done/Rollback activity date, else task `CreatedAt` |
| **Member** | Assigned user name (`Users.Name`) — **non-archived only** |
| **LO Code** | `LearningObjectives.Tag` |
| **Subject** | Derived from `Subjects.Name` prefix (`ara_`, `eng_`, …) |
| **Task** | `TaskBank.Name`, else `Tasks.Name` |
| **Time** | Actual minutes (see scoring rules) |
| **Expected** | Expected minutes for scoring |
| **Points** | 3 / 2 / 1 / 0 (see below) |
| **Status** | Approved / Hold / Rollback |
| **Note** | Latest rollback clarification (if any) |

Default table window: **last 7 days**, **5 rows per page**.

### 4. Duplicate LO tools (client-side)

Loads up to 500 filtered rows and highlights LO codes that appear more than once.

---

## Business rules

### Who is included?

- Task must be **not archived**
- Linked LO / Lesson / Unit / Subject must be **not archived**
- Assigned user must exist and be **`Users.Archived = 0`**
- Unassigned tasks and tasks assigned to archived users are **excluded**
- Work-time sums only include time logged by **non-archived** users

### Role scoping (same idea as Daily Report)

| Role | Sees |
|------|------|
| Owner / Project Manager | All matching tasks |
| Section Head | Tasks in groups under their sections |
| Team Leader | Tasks in their group |
| Member | Tasks assigned to them **or** in their group |

### Status mapping

| Report status | Condition |
|---------------|-----------|
| **Rollback** | `Tasks.Status = Rollback` **or** `IsRollback = 1` |
| **Approved** | `Tasks.Status = Done` |
| **Hold** | Everything else (Backlog, ToDo, Doing, …) |

### Time (ActualMinutes)

1. Prefer sum of `TaskWorkTimes.Duration` for that task (stored in **milliseconds**) ÷ `60000` → minutes  
2. Only work rows from **non-archived** users  
3. If that sum is 0 / missing → fall back to `Tasks.Duration`  
4. Rounded to whole minutes for comparison

### Expected minutes (ExpectedMinutes)

1. Prefer `Steps.Duration` if `> 0`  
2. Else `TaskBank.Duration` if `> 0`  
3. Else `Tasks.Duration` if `> 0`  
4. Else `0` (cannot score)

### Points

| Condition | Points |
|-----------|--------|
| No expected duration (`ExpectedMinutes <= 0`) | **0** |
| Actual **&lt;** Expected | **3** |
| Actual **=** Expected | **2** |
| Actual **&gt;** Expected | **1** |

---

## API endpoints

| Method | Route | Purpose |
|--------|-------|---------|
| `GET` | `/task-logger/dashboard` | One call: paged rows + summary + rankings + lookups |
| `GET` | `/task-logger` | Paged rows only (export / duplicates) |
| `GET` | `/task-logger/lookups` | Member / task-name dropdown data |

### Query parameters

| Param | Used for |
|-------|----------|
| `from`, `to` | Table / summary date window |
| `search` | LO code or task name contains |
| `member`, `subject`, `status`, `taskName` | Exact filters |
| `rankingRange` | `all` \| `week` \| `month` |
| `page`, `pageSize` | Pagination (default 5, max 500) |

---

## Query architecture (high level)

```text
GET /task-logger/dashboard
├── Phase A: Light CTE → COUNT + page of TaskIds
├── Phase B: Enrich those TaskIds (notes + full row fields)
├── Summary: same Light CTE → aggregates
├── Rankings: Rankings CTE → SUM(Points) by Member
└── Lookups: light distinct members / task names
```

All reads use **Dapper + raw SQL** (not EF includes), similar to Daily Report.

---

## Core query explained (`LightBaseCte`)

This is the main building block for rows, summary, and (with a variant) rankings.

### 1) `ActivityDates` CTE

```sql
SELECT TaskId, MAX(TimeStamp) AS ReportDate
FROM TaskActivities
WHERE Type IN (@statusDone, @statusRollback, @rollbackActivity)
  AND TimeStamp >= @fromDate
  AND TimeStamp < DATEADD(DAY, 1, @toDate)
GROUP BY TaskId
```

**Purpose:** Find the latest “completion-like” activity per task inside the date window.

| Part | Meaning |
|------|---------|
| `Type IN (...)` | Only Done / Rollback activity types |
| `TimeStamp` range | Bound the scan to the report window (performance) |
| `MAX(TimeStamp)` | One report date per task |
| `GROUP BY TaskId` | One row per task |

### 2) `WorkTimes` CTE

```sql
SELECT twt.TaskId, SUM(twt.Duration) / 60000.0 AS ActualMinutes
FROM TaskWorkTimes twt
INNER JOIN Users wu ON wu.Id = twt.UserId AND wu.Archived = 0
GROUP BY twt.TaskId
```

**Purpose:** Actual work time in minutes, only from active users.

| Part | Meaning |
|------|---------|
| `SUM(Duration)` | Total logged work (milliseconds) |
| `/ 60000.0` | Convert ms → minutes |
| `wu.Archived = 0` | Ignore archived users’ work logs |

### 3) `BaseRows` CTE — joins

```text
Tasks t
  → Groups g
  → LearningObjectives lo
  → Lessons l
  → Units un
  → Subjects s
  → Users usr          (INNER, Archived = 0)
  → Steps / TaskBank   (LEFT)
  → ActivityDates      (LEFT)
  → WorkTimes          (LEFT)
```

| Join | Why |
|------|-----|
| Hierarchy to `Subjects` | Subject label + ensure LO path exists |
| `INNER JOIN Users … Archived = 0` | Only assigned, active members |
| `LEFT JOIN Steps/TaskBank` | Task name + expected duration when present |
| `LEFT JOIN ActivityDates` | Report date when activity exists |
| `LEFT JOIN WorkTimes` | Actual minutes when work was logged |

**Archive filters on content:**

```sql
WHERE t.Archived = 0 AND lo.Archived = 0 AND l.Archived = 0
  AND un.Archived = 0 AND s.Archived = 0
```

### 4) `BaseRows` — computed columns

#### ReportDate

```sql
CAST(COALESCE(ad.ReportDate, t.CreatedAt) AS date)
```

Use activity date if present; otherwise task creation date.

#### Subject

`CASE` on `Subjects.Name` prefixes (`ara_%` → Arabic, `eng_%` → English, …, else Other).

#### TaskName

```sql
COALESCE(NULLIF(tb.Name, ''), t.Name)
```

Prefer TaskBank name; fall back to task name.

#### ActualMinutes

```sql
ROUND(COALESCE(NULLIF(wt.ActualMinutes, 0), NULLIF(t.Duration, 0), 0), 0)
```

Work times first; else task duration; else 0.

#### ExpectedMinutes

```sql
COALESCE(NULLIF(tb.Duration, 0), NULLIF(t.Duration, 0), 0)
```

TaskBank duration first; else task duration; else 0 (unscored).

#### Points

```sql
CASE
  WHEN ExpectedMinutes <= 0 THEN 0
  WHEN Actual < Expected THEN 4
  WHEN Actual = Expected THEN 3
  ELSE 1.5
END
```

#### Status

```sql
CASE
  WHEN t.Status = Rollback OR t.IsRollback = 1 THEN 'Rollback'
  WHEN t.Status = Done THEN 'Approved'
  ELSE 'Hold'
END
```

### 5) Injected placeholders

| Placeholder | Injected by C# |
|-------------|----------------|
| `{DATE_FILTER_BASE}` | `COALESCE(ad.ReportDate, t.CreatedAt)` between `@fromDate` and `@toDate` |
| `{ROLE_FILTER}` | Role-based `GroupId` / `UserId` restriction |

Example date filter:

```sql
AND COALESCE(ad.ReportDate, t.CreatedAt) >= @fromDate
AND COALESCE(ad.ReportDate, t.CreatedAt) < DATEADD(DAY, 1, @toDate)
```

(`DATEADD(DAY, 1, @toDate)` makes the end date inclusive for the whole day.)

---

## Two-phase paging (rows)

### Phase A — count + TaskIds

```sql
SET NOCOUNT ON;
WITH ... BaseRows AS (...),
Filtered AS (SELECT * FROM BaseRows WHERE ...filters...)
SELECT * INTO #TaskLoggerFiltered FROM Filtered;
SELECT COUNT(1) FROM #TaskLoggerFiltered;
SELECT TaskId
FROM #TaskLoggerFiltered
ORDER BY ReportDate DESC, TaskId DESC
OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
```

| Step | Why |
|------|-----|
| Materialize into `#TaskLoggerFiltered` | SQL CTEs only apply to **one** statement; temp table lets us run COUNT + page together |
| `COUNT` | Total rows for pagination |
| `OFFSET / FETCH` | Current page of IDs only |

### Phase B — enrich page TaskIds (`EnrichRowsSql`)

Runs only for the current page (e.g. 5 IDs):

- Same time / points / subject logic  
- Adds **Notes** from latest rollback clarification (`ROW_NUMBER() … ORDER BY r.Id DESC`)

This avoids joining rollbacks for the entire filtered set just to show one page.

---

## Summary query

Same Light CTE + filters, then:

```sql
SELECT
  COUNT(1) AS TotalTasks,
  ISNULL(SUM(ActualMinutes), 0) AS TotalTimeMinutes,
  SUM(CASE WHEN [Status] = 'Rollback' THEN 1 ELSE 0 END) AS RollbackTasks,
  ISNULL(SUM(Points), 0) AS TotalPoints
FROM #TaskLoggerSummary
```

Feeds the four stats cards.

---

## Rankings query

Uses `RankingsBaseCte` (same points logic, fewer selected columns).

| `rankingRange` | Activity / date behavior |
|----------------|--------------------------|
| `all` | No date bound |
| `week` / `month` | Bound ActivityDates + report-date filter to that window |

Then:

```sql
SELECT TOP 5 Member, SUM(Points) AS Points
FROM Filtered
WHERE Member IS NOT NULL AND Member <> ''
GROUP BY Member
ORDER BY SUM(Points) DESC, Member
```

Only the **top 5** members are returned. Rank number (`1..5`) is assigned in C# after ordering.

---

## Lookups

| Query | Returns |
|-------|---------|
| Members | Distinct active assignee names in date window (role-scoped) |
| Task names | Distinct TaskBank/Task names |
| Subjects / Statuses | Static lists on the server |

---

## Data flow diagram

```text
                    ┌─────────────────────┐
                    │  TaskActivities     │──► ReportDate
                    │  TaskWorkTimes      │──► ActualMinutes
                    │  TaskBank / Tasks   │──► ExpectedMinutes
                    │  Users (active)     │──► Member
                    │  LO.Tag / Subjects  │──► LoCode / Subject
                    └─────────┬───────────┘
                              │
                              ▼
                     Light CTE → BaseRows
                              │
          ┌───────────────────┼───────────────────┐
          ▼                   ▼                   ▼
     Paged TaskIds        Summary stats      Member rankings
          │
          ▼
   Enrich (Notes) → Table rows
```

---

## Important files to read when changing behavior

1. Scoring / joins → `TaskLoggerSql.cs`  
2. Role filter / ranking range / pagination → `TaskLoggerService.cs`  
3. HTTP contract → `TaskLoggerController.cs`  
4. UI filters / rankings / duplicates → `components/taskLogger/`
