namespace AutomatedTaskSystem.Services.DailyReport;

internal static class DailyReportSql
{
    /// <summary>Minimal CTE for counts, aggregates, and paged TaskIds — no rollback/note joins.</summary>
    internal const string LightBaseCte = """
        WITH ActivityDates AS (
            SELECT TaskId, MAX(TimeStamp) AS ReportDate
            FROM TaskActivities
            WHERE Type IN (@statusDone, @statusRollback, @rollbackActivity)
              AND TimeStamp >= @fromDate
              AND TimeStamp < DATEADD(DAY, 1, @toDate)
            GROUP BY TaskId
        ),
        BaseRows AS (
            SELECT
                t.Id AS TaskId,
                CAST(COALESCE(ad.ReportDate, t.CreatedAt) AS date) AS ReportDate,
                g.Name AS Team,
                CASE
                    WHEN s.Name LIKE '%[_]2a' OR s.Name LIKE '%[_]2e'
                      OR LOWER(s.Name) LIKE '%term2%'
                      OR LOWER(s.Name) LIKE '%term[_]2%'
                    THEN 'Term 2'
                    ELSE 'Term 1'
                END AS Semester,
                CASE
                    WHEN s.Name LIKE 'ara[_]%' THEN 'Arabic'
                    WHEN s.Name LIKE 'eng[_]%' THEN 'English'
                    WHEN s.Name LIKE 'mth[_]%' THEN 'Math (A)'
                    WHEN s.Name LIKE 'sci[_]%' THEN 'Science (A)'
                    WHEN s.Name LIKE 'soc[_]%' THEN 'Social'
                    WHEN s.Name LIKE 'ict[_]%' THEN 'ICT (A)'
                    WHEN s.Name LIKE 'mul[_]%' THEN 'MUL (A)'
                    WHEN s.Name LIKE 'rel[_]%' THEN 'Religion'
                    ELSE 'Other'
                END AS Subjects,
                CASE LOWER(parsed.GradeCode)
                    WHEN '1k' THEN 'Kg1' WHEN '2k' THEN 'Kg2'
                    WHEN '1r' THEN 'Grade 1' WHEN '2r' THEN 'Grade 2' WHEN '3r' THEN 'Grade 3'
                    WHEN '4r' THEN 'Grade 4' WHEN '5r' THEN 'Grade 5' WHEN '6r' THEN 'Grade 6'
                    WHEN '7r' THEN 'Grade 7'
                    ELSE parsed.GradeCode
                END AS Grade,
                COALESCE(NULLIF(tb.Name, ''), t.Name) AS TaskName,
                CASE
                    WHEN t.Flagged = 1 THEN 'Red Flag'
                    WHEN t.Status = @taskRollback OR t.IsRollback = 1 THEN 'Rollback'
                    WHEN t.Status = @taskDone THEN 'Approved'
                    ELSE 'Hold'
                END AS [Status],
                CASE t.Priority
                    WHEN @priorityHigh THEN 'High'
                    WHEN @priorityMedium THEN 'Medium'
                    WHEN @priorityLow THEN 'Low'
                    ELSE 'None'
                END AS [Priority]
            FROM Tasks t
            INNER JOIN Groups g ON g.Id = t.GroupId
            INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId
            INNER JOIN Lessons l ON l.Id = lo.LessonId
            INNER JOIN Units un ON un.Id = l.UnitId
            INNER JOIN Subjects s ON s.Id = un.SubjectId
            LEFT JOIN Steps st ON st.Id = t.StepId
            LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
            LEFT JOIN ActivityDates ad ON ad.TaskId = t.Id
            CROSS APPLY (SELECT FirstUnderScore = CHARINDEX('_', s.Name)) firstPosition
            CROSS APPLY (
                SELECT SecondUnderScore = CHARINDEX('_', s.Name, firstPosition.FirstUnderScore + 1)
            ) secondPosition
            CROSS APPLY (
                SELECT GradeCode =
                    CASE
                        WHEN firstPosition.FirstUnderScore > 0
                         AND secondPosition.SecondUnderScore > firstPosition.FirstUnderScore
                        THEN SUBSTRING(
                            s.Name,
                            firstPosition.FirstUnderScore + 1,
                            secondPosition.SecondUnderScore - firstPosition.FirstUnderScore - 1)
                        ELSE ''
                    END
            ) parsed
            WHERE t.Archived = 0 AND lo.Archived = 0 AND l.Archived = 0
              AND un.Archived = 0 AND s.Archived = 0
              {DATE_FILTER_BASE}
              {ROLE_FILTER}
        )
        """;

    internal const string FilteredRows = """
        SELECT * FROM BaseRows
        WHERE 1 = 1
          {TEAM_FILTER}
          {SEMESTER_FILTER}
          {SUBJECT_FILTER}
          {GRADE_FILTER}
          {TASK_FILTER}
          {STATUS_FILTER}
          {PROBLEM_FILTER}
          {PRIORITY_FILTER}
        """;

    /// <summary>
    /// CTEs only apply to one statement — materialize once, then return two result sets.
    /// Prefix with LightBaseCte + Filtered AS (...).
    /// </summary>
    internal const string CountAndPagedTaskIds = """
        SELECT * INTO #DailyReportFiltered FROM Filtered;
        SELECT COUNT(1) FROM #DailyReportFiltered;
        SELECT TaskId
        FROM #DailyReportFiltered
        ORDER BY ReportDate DESC, TaskId DESC
        OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY
        """;

    /// <summary>Full row detail for a small set of TaskIds only (current page).</summary>
    internal const string EnrichRowsSql = """
        WITH LatestRollback AS (
            SELECT r.TaskId, r.Clarification, tg.Name AS ToGroupName,
                ROW_NUMBER() OVER (PARTITION BY r.TaskId ORDER BY r.Id DESC) AS rn
            FROM Rollbacks r
            LEFT JOIN Tasks tt ON tt.Id = r.ToTaskId
            LEFT JOIN Groups tg ON tg.Id = tt.GroupId
            WHERE r.TaskId IN @taskIds
        ),
        RollbackProblem AS (
            SELECT agg.TaskId,
                STRING_AGG(agg.ProblemType, ', ') WITHIN GROUP (ORDER BY agg.ProblemType) AS ProblemType
            FROM (
                SELECT DISTINCT r.TaskId,
                    LTRIM(RTRIM(split.value)) AS ProblemType
                FROM Rollbacks r
                LEFT JOIN RollbackIssues ri ON ri.RollbackId = r.Id
                LEFT JOIN Steps st ON st.Id = ri.StepId
                LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
                CROSS APPLY STRING_SPLIT(
                    CASE WHEN ISNULL(r.ProblemType, '') <> '' THEN r.ProblemType ELSE ISNULL(tb.Name, '') END,
                    ','
                ) split
                WHERE r.TaskId IN @taskIds
                  AND LTRIM(RTRIM(split.value)) <> ''
            ) agg
            GROUP BY agg.TaskId
        ),
        ActivityDates AS (
            SELECT TaskId, MAX(TimeStamp) AS ReportDate
            FROM TaskActivities
            WHERE TaskId IN @taskIds
              AND Type IN (@statusDone, @statusRollback, @rollbackActivity)
            GROUP BY TaskId
        )
        SELECT
            t.Id AS TaskId,
            CONVERT(varchar(10), CAST(COALESCE(ad.ReportDate, t.CreatedAt) AS date), 23) AS [Date],
            g.Name AS Team,
            CASE
                WHEN s.Name LIKE '%[_]2a' OR s.Name LIKE '%[_]2e'
                  OR LOWER(s.Name) LIKE '%term2%'
                  OR LOWER(s.Name) LIKE '%term[_]2%'
                THEN 'Term 2'
                ELSE 'Term 1'
            END AS Semester,
            CASE
                WHEN s.Name LIKE 'ara[_]%' THEN 'Arabic'
                WHEN s.Name LIKE 'eng[_]%' THEN 'English'
                WHEN s.Name LIKE 'mth[_]%' THEN 'Math (A)'
                WHEN s.Name LIKE 'sci[_]%' THEN 'Science (A)'
                WHEN s.Name LIKE 'soc[_]%' THEN 'Social'
                WHEN s.Name LIKE 'ict[_]%' THEN 'ICT (A)'
                WHEN s.Name LIKE 'mul[_]%' THEN 'MUL (A)'
                WHEN s.Name LIKE 'rel[_]%' THEN 'Religion'
                ELSE 'Other'
            END AS Subjects,
            CASE LOWER(parsed.GradeCode)
                WHEN '1k' THEN 'Kg1' WHEN '2k' THEN 'Kg2'
                WHEN '1r' THEN 'Grade 1' WHEN '2r' THEN 'Grade 2' WHEN '3r' THEN 'Grade 3'
                WHEN '4r' THEN 'Grade 4' WHEN '5r' THEN 'Grade 5' WHEN '6r' THEN 'Grade 6'
                WHEN '7r' THEN 'Grade 7'
                ELSE parsed.GradeCode
            END AS Grade,
            COALESCE(NULLIF(tb.Name, ''), t.Name) AS TaskName,
            ISNULL(lo.Name, '') AS LoCode,
            ISNULL(sch.Name, '') AS LoType,
            CASE
                WHEN t.Status = @taskRollback OR t.IsRollback = 1 THEN
                    COALESCE(NULLIF(usr.Name, ''), NULLIF(lr.ToGroupName, ''), g.Name, '')
                ELSE ''
            END AS AssignedTo,
            CASE
                WHEN t.Flagged = 1 THEN 'Red Flag'
                WHEN t.Status = @taskRollback OR t.IsRollback = 1 THEN 'Rollback'
                WHEN t.Status = @taskDone THEN 'Approved'
                ELSE 'Hold'
            END AS [Status],
            ISNULL(rp.ProblemType, '') AS ProblemType,
            CASE t.Priority
                WHEN @priorityHigh THEN 'High'
                WHEN @priorityMedium THEN 'Medium'
                WHEN @priorityLow THEN 'Low'
                ELSE 'None'
            END AS [Priority],
            COALESCE(NULLIF(notes.Notes, ''), lr.Clarification, '') AS Notes
        FROM Tasks t
        INNER JOIN Groups g ON g.Id = t.GroupId
        INNER JOIN LearningObjectives lo ON lo.Id = t.LearningObjectiveId
        INNER JOIN Schemas sch ON sch.Id = lo.SchemaId
        INNER JOIN Lessons l ON l.Id = lo.LessonId
        INNER JOIN Units un ON un.Id = l.UnitId
        INNER JOIN Subjects s ON s.Id = un.SubjectId
        LEFT JOIN Users usr ON usr.Id = t.UserId
        LEFT JOIN Steps st ON st.Id = t.StepId
        LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
        LEFT JOIN ActivityDates ad ON ad.TaskId = t.Id
        LEFT JOIN DailyReportNoteOverrides notes ON notes.TaskId = t.Id
        LEFT JOIN LatestRollback lr ON lr.TaskId = t.Id AND lr.rn = 1
        LEFT JOIN RollbackProblem rp ON rp.TaskId = t.Id
        CROSS APPLY (SELECT FirstUnderScore = CHARINDEX('_', s.Name)) firstPosition
        CROSS APPLY (
            SELECT SecondUnderScore = CHARINDEX('_', s.Name, firstPosition.FirstUnderScore + 1)
        ) secondPosition
        CROSS APPLY (
            SELECT GradeCode =
                CASE
                    WHEN firstPosition.FirstUnderScore > 0
                     AND secondPosition.SecondUnderScore > firstPosition.FirstUnderScore
                    THEN SUBSTRING(
                        s.Name,
                        firstPosition.FirstUnderScore + 1,
                        secondPosition.SecondUnderScore - firstPosition.FirstUnderScore - 1)
                    ELSE ''
                END
        ) parsed
        WHERE t.Id IN @taskIds
        """;

    /// <summary>
    /// CTEs only apply to one statement — materialize once, then return two result sets.
    /// Prefix with LightBaseCte + Filtered AS (...).
    /// </summary>
    internal const string SummaryAndTopTeams = """
        SELECT * INTO #DailyReportSummary FROM Filtered;
        SELECT
            COUNT(1) AS Total,
            SUM(CASE WHEN [Status] = 'Approved' THEN 1 ELSE 0 END) AS Approved,
            SUM(CASE WHEN [Status] = 'Hold' THEN 1 ELSE 0 END) AS [Hold],
            SUM(CASE WHEN [Status] = 'Rollback' THEN 1 ELSE 0 END) AS [Rollback],
            COUNT(DISTINCT NULLIF(Team, '')) AS ActiveTeams
        FROM #DailyReportSummary;
        SELECT TOP 4 Team, COUNT(1) AS [Count]
        FROM #DailyReportSummary
        WHERE Team IS NOT NULL AND Team <> ''
        GROUP BY Team
        ORDER BY COUNT(1) DESC
        """;

    internal const string ProblemTypeExistsFilter = """
        AND EXISTS (
            SELECT 1
            FROM Rollbacks r
            LEFT JOIN RollbackIssues ri ON ri.RollbackId = r.Id
            LEFT JOIN Steps pst ON pst.Id = ri.StepId
            LEFT JOIN TaskBank ptb ON ptb.Id = pst.TaskBankId
            CROSS APPLY STRING_SPLIT(
                CASE WHEN ISNULL(r.ProblemType, '') <> '' THEN r.ProblemType ELSE ISNULL(ptb.Name, '') END,
                ','
            ) split
            WHERE r.TaskId = TaskId
              AND LTRIM(RTRIM(split.value)) IN @problemTypes
        )
        """;

    /// <summary>Problem-type chart — only rollback tasks, no full report CTE.</summary>
    internal const string ProblemTypesDirect = """
        SELECT LTRIM(RTRIM(split.value)) AS ProblemType,
               COUNT(DISTINCT r.Id) AS [Count]
        FROM Tasks t
        INNER JOIN Rollbacks r ON r.TaskId = t.Id
        LEFT JOIN RollbackIssues ri ON ri.RollbackId = r.Id
        LEFT JOIN Steps st ON st.Id = ri.StepId
        LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
        CROSS APPLY STRING_SPLIT(
            CASE WHEN ISNULL(r.ProblemType, '') <> '' THEN r.ProblemType ELSE ISNULL(tb.Name, '') END,
            ','
        ) split
        WHERE t.Archived = 0
          AND (t.Status = @taskRollback OR t.IsRollback = 1)
          AND t.CreatedAt >= @fromDate
          AND t.CreatedAt < DATEADD(DAY, 1, @toDate)
          AND LTRIM(RTRIM(split.value)) <> ''
          {ROLE_FILTER}
        GROUP BY LTRIM(RTRIM(split.value))
        ORDER BY COUNT(DISTINCT r.Id) DESC
        """;

    internal const string LightLookupsTeams = """
        SELECT DISTINCT g.Name AS Value
        FROM Tasks t
        INNER JOIN Groups g ON g.Id = t.GroupId
        WHERE t.Archived = 0
          AND t.CreatedAt >= @fromDate
          AND t.CreatedAt < DATEADD(DAY, 1, @toDate)
          {ROLE_FILTER}
        ORDER BY g.Name
        """;

    internal const string LightLookupsTaskNames = """
        SELECT DISTINCT TOP 200 COALESCE(NULLIF(tb.Name, ''), t.Name) AS Value
        FROM Tasks t
        LEFT JOIN Steps st ON st.Id = t.StepId
        LEFT JOIN TaskBank tb ON tb.Id = st.TaskBankId
        WHERE t.Archived = 0
          AND t.CreatedAt >= @fromDate
          AND t.CreatedAt < DATEADD(DAY, 1, @toDate)
          {TEAM_FILTER}
          {ROLE_FILTER}
        ORDER BY Value
        """;
}
