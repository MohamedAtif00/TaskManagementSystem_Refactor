# Sprints and Projects - All Queries

This file centralizes all sprint/project analytics queries found in the current implementation.

---

## 1) Sprint Overview API Query

**Type:** HTTP GET  
**Title:** Get Sprint Overview Analytics  
**Description:** Returns high-level sprint analytics including task summary, LO summary, tags distribution, and sprint summary.  
**Query:**

```http
GET /sprints/analytics/overview?sprintId={sprintId}&timePeriod={timePeriod}
```

**Parameters:**
- `sprintId` (required): Sprint ID.
- `timePeriod` (optional): `1=Today`, `2=Last Week`, `3=Last Month`, `4=All Time`.

---

## 2) Learning Objectives Progress API Query

**Type:** HTTP GET  
**Title:** Get Sprint Learning Objectives Progress  
**Description:** Returns progress/status per learning objective in a sprint, with optional time period and group filtering.  
**Query:**

```http
GET /sprints/analytics/learning-objectives-progress?sprintId={sprintId}&timePeriod={timePeriod}&group={groupId}
```

**Parameters:**
- `sprintId` (required): Sprint ID.
- `timePeriod` (optional): `1=Today`, `2=Last Week`, `3=Last Month`, `4=All Time`.
- `group` (optional): Group/Tag ID filter.

---

## 3) Learning Objectives Table API Query

**Type:** HTTP GET  
**Title:** Get Sprint Learning Objectives Table  
**Description:** Returns table-ready data for learning objectives in a sprint (name, dates, active tasks, phases, status, progress).  
**Query:**

```http
GET /sprints/analytics/learning-objectives-table?sprintId={sprintId}
```

**Parameters:**
- `sprintId` (required): Sprint ID.

---

## 4) SQL Query - Total Expected Tasks

**Type:** SQL  
**Title:** Count Expected Tasks from Schema Steps  
**Description:** Counts expected tasks for sprint learning objectives based on schema nodes/steps (excluding archived and "old" LOs).  
**Query:**

```sql
SELECT
    TotalExpectedTasks = COUNT(1)
FROM SprintLearningObjectives slo
INNER JOIN LearningObjectives lo ON lo.Id = slo.LearningObjectiveId AND lo.Archived = 0
INNER JOIN Nodes n ON n.SchemaId = lo.SchemaId AND n.Archived = 0
INNER JOIN Steps st ON st.NodeId = n.Id AND st.Archived = 0
WHERE slo.SprintId = @sprintId
  AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%';
```

---

## 5) SQL Query - Sprint Task Summary

**Type:** SQL  
**Title:** Aggregate Task Status Summary  
**Description:** Aggregates completed, active, rollback, and flagged task counts for the selected sprint.  
**Query:**

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

---

## 6) SQL Query - Tags Distribution

**Type:** SQL  
**Title:** Group/Tag Distribution for Active Pipeline Tasks  
**Description:** Returns counts of non-done tasks by group/tag for sprint analytics charts, with optional date filtering.  
**Query:**

```sql
SELECT
    t.GroupId AS GroupId,
    g.Name AS [Label],
    g.ColorCode AS ColorCode,
    COUNT(1) AS [Value]
FROM Tasks t
INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId AND lo.Archived = 0
INNER JOIN SprintLearningObjectives slo ON slo.LearningObjectiveId = lo.Id AND slo.SprintId = @sprintId
INNER JOIN Groups g ON g.Id = t.GroupId
WHERE t.Archived = 0
  AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
  AND t.Status IN (@backlog, @todo, @doing)
  AND (@startDate IS NULL OR (t.CreatedAt >= @startDate AND t.CreatedAt <= @endDate))
GROUP BY t.GroupId, g.Name, g.ColorCode;
```

---

## 7) SQL Query - Learning Objectives Summary

**Type:** SQL  
**Title:** Categorize LOs into Completed / Not Started / In Process  
**Description:** Uses CTEs to calculate LO completion state for sprint-level summary charts.  
**Query:**

```sql
WITH SprintLOs AS (
    SELECT lo.Id, lo.SchemaId
    FROM SprintLearningObjectives slo
    INNER JOIN LearningObjectives lo ON lo.Id = slo.LearningObjectiveId
    WHERE slo.SprintId = @sprintId
      AND lo.Archived = 0
      AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
),
ExpectedSteps AS (
    SELECT sl.Id AS LearningObjectiveId, COUNT(1) AS ExpectedCount
    FROM SprintLOs sl
    INNER JOIN Nodes n ON n.SchemaId = sl.SchemaId AND n.Archived = 0
    INNER JOIN Steps s ON s.NodeId = n.Id AND s.Archived = 0
    GROUP BY sl.Id
),
TaskAgg AS (
    SELECT t.LearningObjectiveId,
           TotalTasks = COUNT(1),
           NonBacklogTasks = SUM(CASE WHEN t.Status <> @backlog THEN 1 ELSE 0 END),
           NotDoneTasks = SUM(CASE WHEN t.Status <> @done THEN 1 ELSE 0 END)
    FROM Tasks t
    INNER JOIN SprintLOs sl ON sl.Id = t.LearningObjectiveId
    WHERE t.Archived = 0
    GROUP BY t.LearningObjectiveId
)
SELECT
    Total = (SELECT COUNT(1) FROM SprintLOs),
    Completed = SUM(CASE WHEN ISNULL(ta.TotalTasks, 0) > 0 AND ISNULL(ta.NotDoneTasks, 0) = 0 THEN 1 ELSE 0 END),
    NotStarted = SUM(CASE
        WHEN ISNULL(ta.TotalTasks, 0) = 0 AND ISNULL(es.ExpectedCount, 0) > 0 THEN 1
        WHEN ISNULL(ta.TotalTasks, 0) > 0 AND ISNULL(ta.NonBacklogTasks, 0) = 0 THEN 1
        ELSE 0
    END)
FROM SprintLOs sl
LEFT JOIN ExpectedSteps es ON es.LearningObjectiveId = sl.Id
LEFT JOIN TaskAgg ta ON ta.LearningObjectiveId = sl.Id;
```

---

## 8) SQL Query - Sprint Name Lookup

**Type:** SQL  
**Title:** Get Sprint Name  
**Description:** Retrieves sprint name for table/report headers and validation checks.  
**Query:**

```sql
SELECT Name
FROM Sprints
WHERE Id = @sprintId;
```

---

## 9) SQL Query - Learning Objectives Table Rows

**Type:** SQL  
**Title:** Build LO Table Core Data  
**Description:** Produces the main rows for LO analytics table with expected/completed/active task counts per LO.  
**Query:**

```sql
WITH SprintLOs AS (
    SELECT lo.Id, lo.Name, lo.StartedAt, lo.DoneAt, lo.SchemaId
    FROM SprintLearningObjectives slo
    INNER JOIN LearningObjectives lo ON lo.Id = slo.LearningObjectiveId
    WHERE slo.SprintId = @sprintId
      AND lo.Archived = 0
      AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
),
ExpectedSteps AS (
    SELECT sl.Id AS LearningObjectiveId, COUNT(1) AS TotalExpectedTasks
    FROM SprintLOs sl
    INNER JOIN Nodes n ON n.SchemaId = sl.SchemaId AND n.Archived = 0
    INNER JOIN Steps s ON s.NodeId = n.Id AND s.Archived = 0
    GROUP BY sl.Id
),
TaskAgg AS (
    SELECT t.LearningObjectiveId,
           CompletedTasks = SUM(CASE WHEN t.Status = @done THEN 1 ELSE 0 END),
           ActiveTasks = SUM(CASE WHEN t.Status IN (@todo, @doing) THEN 1 ELSE 0 END)
    FROM Tasks t
    INNER JOIN SprintLOs sl ON sl.Id = t.LearningObjectiveId
    WHERE t.Archived = 0
    GROUP BY t.LearningObjectiveId
)
SELECT
    sl.Id,
    sl.Name,
    sl.StartedAt,
    sl.DoneAt,
    TotalExpectedTasks = ISNULL(es.TotalExpectedTasks, 0),
    CompletedTasks = ISNULL(ta.CompletedTasks, 0),
    ActiveTasks = ISNULL(ta.ActiveTasks, 0)
FROM SprintLOs sl
LEFT JOIN ExpectedSteps es ON es.LearningObjectiveId = sl.Id
LEFT JOIN TaskAgg ta ON ta.LearningObjectiveId = sl.Id
ORDER BY sl.Id;
```

---

## 10) SQL Query - Current Phases per Learning Objective

**Type:** SQL  
**Title:** Fetch Active Group Phases by LO  
**Description:** Returns active group phases (backlog/todo/doing) per learning objective for table phase indicators.  
**Query:**

```sql
SELECT
    lo.Id AS LearningObjectiveId,
    g.Name AS GroupName,
    g.ColorCode AS ColorCode
FROM SprintLearningObjectives slo
INNER JOIN LearningObjectives lo ON lo.Id = slo.LearningObjectiveId AND lo.Archived = 0
INNER JOIN Tasks t ON t.LearningObjectiveId = lo.Id AND t.Archived = 0
INNER JOIN Groups g ON g.Id = t.GroupId
WHERE slo.SprintId = @sprintId
  AND LOWER(ISNULL(lo.Name, '')) NOT LIKE '%old%'
  AND t.Status IN (@backlog, @todo, @doing);
```

